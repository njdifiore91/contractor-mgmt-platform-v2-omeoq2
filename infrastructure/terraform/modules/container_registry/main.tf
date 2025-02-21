# Azure Container Registry Module
# Provider version: hashicorp/azurerm ~> 3.0

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# Random string for unique ACR name
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                          = "${var.environment}acr${random_string.suffix.result}"
  resource_group_name           = var.resource_group_name
  location                      = var.location
  sku                          = var.sku
  admin_enabled                 = true
  public_network_access_enabled = false

  network_rule_set {
    default_action    = var.network_rule_set.default_action
    ip_rules         = var.network_rule_set.ip_rules
    virtual_network_subnet_ids = var.network_rule_set.virtual_network_rules
  }

  retention_policy {
    days    = var.retention_days
    enabled = true
  }

  dynamic "georeplications" {
    for_each = var.sku == "Premium" ? var.georeplications : []
    content {
      location = georeplications.value.location
      tags     = georeplications.value.tags
    }
  }

  encryption {
    enabled = true
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "Terraform"
    Purpose     = "Container Registry"
  }
}

# Diagnostic settings for ACR
resource "azurerm_monitor_diagnostic_setting" "acr_diagnostics" {
  name                       = "${azurerm_container_registry.acr.name}-diagnostics"
  target_resource_id         = azurerm_container_registry.acr.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "ContainerRegistryRepositoryEvents"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Outputs
output "acr_name" {
  description = "The name of the Azure Container Registry"
  value       = azurerm_container_registry.acr.name
}

output "acr_login_server" {
  description = "The login server URL of the Azure Container Registry"
  value       = azurerm_container_registry.acr.login_server
}

output "acr_admin_username" {
  description = "The admin username for the Azure Container Registry"
  value       = azurerm_container_registry.acr.admin_username
}

output "acr_id" {
  description = "The resource ID of the Azure Container Registry"
  value       = azurerm_container_registry.acr.id
}

output "acr_admin_password" {
  description = "The admin password for the Azure Container Registry"
  value       = azurerm_container_registry.acr.admin_password
  sensitive   = true
}