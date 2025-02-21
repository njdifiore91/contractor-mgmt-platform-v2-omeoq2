# Output configuration for Azure infrastructure resources
# Exposes critical infrastructure details with enhanced security and validation

# Resource Group Output
output "resource_group_name" {
  description = "Name of the Azure Resource Group containing all infrastructure resources"
  value       = azurerm_resource_group.main.name
  sensitive   = false

  # Ensure resource group name follows naming convention
  validation {
    condition     = can(regex("^[a-z0-9-]{3,63}$", azurerm_resource_group.main.name))
    error_message = "Resource group name must be 3-63 characters long and contain only lowercase letters, numbers, and hyphens."
  }
}

# App Service URLs with HTTPS validation
output "web_app_url" {
  description = "Public HTTPS endpoint for the Vue.js frontend application"
  value       = module.app_service_module.web_app_url

  # Ensure URL uses HTTPS protocol
  validation {
    condition     = can(regex("^https://", module.app_service_module.web_app_url))
    error_message = "Web app URL must use HTTPS protocol."
  }
}

output "api_app_url" {
  description = "Public HTTPS endpoint for the ASP.NET Core backend API"
  value       = module.app_service_module.api_app_url

  # Ensure URL uses HTTPS protocol
  validation {
    condition     = can(regex("^https://", module.app_service_module.api_app_url))
    error_message = "API app URL must use HTTPS protocol."
  }
}

# SQL Server details with enhanced security markers
output "sql_server_name" {
  description = "Name of the Azure SQL Server instance (sensitive resource)"
  value       = module.sql_server_module.server_name
  sensitive   = true
}

output "sql_database_name" {
  description = "Name of the application database in Azure SQL Server (sensitive resource)"
  value       = module.sql_server_module.database_name
  sensitive   = true
}

# Key Vault details with security validation
output "key_vault_name" {
  description = "Name of the Azure Key Vault for secrets management"
  value       = module.key_vault_module.key_vault_name
  sensitive   = true

  # Ensure Key Vault name follows security requirements
  validation {
    condition     = can(regex("^[a-zA-Z0-9-]{3,24}$", module.key_vault_module.key_vault_name))
    error_message = "Key Vault name must be 3-24 characters long and contain only alphanumeric characters and hyphens."
  }
}

# Container Registry details with URL validation
output "container_registry_name" {
  description = "Name of the Azure Container Registry"
  value       = module.container_registry_module.registry_name
  sensitive   = false

  # Ensure registry name follows ACR naming requirements
  validation {
    condition     = can(regex("^[a-zA-Z0-9]{5,50}$", module.container_registry_module.registry_name))
    error_message = "Container Registry name must be 5-50 characters long and contain only alphanumeric characters."
  }
}

output "container_registry_login_server" {
  description = "Login server URL for the Azure Container Registry"
  value       = module.container_registry_module.registry_login_server
  sensitive   = false

  # Ensure login server uses valid domain format
  validation {
    condition     = can(regex("^[a-zA-Z0-9]+\\.azurecr\\.io$", module.container_registry_module.registry_login_server))
    error_message = "Container Registry login server must be a valid Azure Container Registry domain."
  }
}