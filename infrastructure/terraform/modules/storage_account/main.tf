# Azure Storage Account Module
# Provider versions:
# azurerm ~> 3.0
# random ~> 3.0

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

locals {
  # Storage account name must be between 3 and 24 characters in length and use numbers and lower-case letters only
  storage_account_name_prefix = lower(var.environment)
  
  default_tags = {
    Environment = var.environment
    Purpose     = "Application Storage"
    ManagedBy   = "Terraform"
    Project     = "Inspector Management System"
  }

  # CORS configuration for web access
  cors_rules = {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "HEAD", "POST", "PUT", "DELETE"]
    allowed_origins = var.allowed_origins
    exposed_headers = ["*"]
    max_age_in_seconds = 3600
  }

  # Default lifecycle management policy
  lifecycle_rules = [
    {
      name = "archive-after-90-days"
      enabled = true
      filters = {
        prefix_match = ["archive/"]
        blob_types   = ["blockBlob"]
      }
      actions = {
        base_blob = {
          tier_to_cool_after_days    = 30
          tier_to_archive_after_days = 90
          delete_after_days          = 365
        }
      }
    }
  ]
}

# Generate random suffix for globally unique storage account name
resource "random_string" "storage_suffix" {
  length  = 6
  special = false
  upper   = false
}

# Main storage account resource
resource "azurerm_storage_account" "main" {
  name                     = "${local.storage_account_name_prefix}${random_string.storage_suffix.result}"
  resource_group_name      = var.resource_group_name
  location                = var.location
  account_tier            = "Standard"
  account_replication_type = "GRS"
  
  # Security settings
  enable_https_traffic_only = true
  min_tls_version          = "TLS1_2"
  allow_nested_items_to_be_public = false

  # Network rules
  network_rules {
    default_action = "Deny"
    ip_rules       = var.allowed_ips
    virtual_network_subnet_ids = var.allowed_subnet_ids
    bypass         = ["AzureServices"]
  }

  # Blob service properties
  blob_properties {
    cors_rule {
      allowed_headers    = local.cors_rules.allowed_headers
      allowed_methods    = local.cors_rules.allowed_methods
      allowed_origins    = local.cors_rules.allowed_origins
      exposed_headers    = local.cors_rules.exposed_headers
      max_age_in_seconds = local.cors_rules.max_age_in_seconds
    }

    delete_retention_policy {
      days = 7
    }

    container_delete_retention_policy {
      days = 7
    }

    versioning_enabled = true
  }

  # Identity
  identity {
    type = "SystemAssigned"
  }

  # Lifecycle management
  dynamic "lifecycle_rule" {
    for_each = local.lifecycle_rules
    content {
      name    = lifecycle_rule.value.name
      enabled = lifecycle_rule.value.enabled
      filters {
        prefix_match = lifecycle_rule.value.filters.prefix_match
        blob_types   = lifecycle_rule.value.filters.blob_types
      }
      actions {
        base_blob {
          tier_to_cool_after_days    = lifecycle_rule.value.actions.base_blob.tier_to_cool_after_days
          tier_to_archive_after_days = lifecycle_rule.value.actions.base_blob.tier_to_archive_after_days
          delete_after_days          = lifecycle_rule.value.actions.base_blob.delete_after_days
        }
      }
    }
  }

  tags = merge(local.default_tags, var.tags)
}

# Create storage containers
resource "azurerm_storage_container" "containers" {
  for_each = var.containers

  name                  = each.key
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = each.value.access_type

  metadata = merge(
    {
      created_by = "terraform"
      purpose    = each.value.purpose
    },
    each.value.metadata
  )
}

# Outputs
output "storage_account_id" {
  description = "The ID of the Storage Account"
  value       = azurerm_storage_account.main.id
}

output "storage_account_name" {
  description = "The name of the Storage Account"
  value       = azurerm_storage_account.main.name
}

output "primary_access_key" {
  description = "The primary access key for the Storage Account"
  value       = azurerm_storage_account.main.primary_access_key
  sensitive   = true
}