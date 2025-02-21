# Azure Key Vault Module
# Provider versions
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm" # version ~> 3.0
    }
    random = {
      source  = "hashicorp/random" # version ~> 3.0
    }
  }
}

# Local variables for resource naming and default configurations
locals {
  key_vault_name = "${var.resource_name_prefix}-kv-${var.environment}-${random_string.suffix.result}"
  
  default_network_acls = {
    bypass                     = "AzureServices"
    default_action            = "Deny"
    ip_rules                  = []
    virtual_network_subnet_ids = []
  }
}

# Generate random suffix for globally unique Key Vault naming
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# Azure Key Vault with enhanced security features
resource "azurerm_key_vault" "vault" {
  name                            = local.key_vault_name
  location                        = var.location
  resource_group_name             = var.resource_group_name
  tenant_id                       = var.tenant_id
  sku_name                        = var.key_vault_sku
  enabled_for_disk_encryption     = true
  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  soft_delete_retention_days      = 90
  purge_protection_enabled        = true
  enable_rbac_authorization       = true

  network_acls {
    bypass                     = local.default_network_acls.bypass
    default_action            = local.default_network_acls.default_action
    ip_rules                  = local.default_network_acls.ip_rules
    virtual_network_subnet_ids = local.default_network_acls.virtual_network_subnet_ids
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# Outputs for reference by other resources
output "key_vault_name" {
  description = "The name of the Key Vault"
  value       = azurerm_key_vault.vault.name
}

output "key_vault_id" {
  description = "The ID of the Key Vault"
  value       = azurerm_key_vault.vault.id
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = azurerm_key_vault.vault.vault_uri
}