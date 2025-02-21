# Environment variable to control deployment environment
variable "environment" {
  type        = string
  description = "Deployment environment (dev, staging, prod)"
  validation {
    condition     = contains(["dev", "staging", "prod"], lower(var.environment))
    error_message = "Environment must be one of: dev, staging, prod"
  }
}

# Azure region for resource deployment
variable "location" {
  type        = string
  description = "Azure region where resources will be deployed"
  validation {
    condition     = contains(["eastus", "eastus2", "westus", "westus2", "centralus", "northcentralus", "southcentralus"], lower(var.location))
    error_message = "Location must be a valid Azure region in the United States"
  }
}

# Resource naming prefix for consistent naming across resources
variable "resource_name_prefix" {
  type        = string
  description = "Prefix used for naming Azure resources"
  validation {
    condition     = can(regex("^[a-z0-9]{3,16}$", var.resource_name_prefix))
    error_message = "Resource prefix must be 3-16 characters long and contain only lowercase letters and numbers"
  }
}

# SQL Server administrative credentials
variable "sql_server_admin_username" {
  type        = string
  description = "Administrative username for Azure SQL Server"
  validation {
    condition     = can(regex("^[a-zA-Z][a-zA-Z0-9]{3,}$", var.sql_server_admin_username))
    error_message = "SQL admin username must start with a letter, be at least 4 characters long, and contain only alphanumeric characters"
  }
}

variable "sql_server_admin_password" {
  type        = string
  description = "Administrative password for Azure SQL Server"
  sensitive   = true
  validation {
    condition     = can(regex("^[a-zA-Z0-9!@#$%^&*()_+=-]{12,128}$", var.sql_server_admin_password))
    error_message = "SQL admin password must be 12-128 characters long and contain a mix of uppercase, lowercase, numbers, and special characters"
  }
}

# App Service configuration
variable "app_service_sku" {
  type        = string
  description = "SKU for App Service Plan (determines pricing tier and features)"
  default     = "P1v2"
  validation {
    condition     = contains(["B1", "B2", "B3", "S1", "S2", "S3", "P1v2", "P2v2", "P3v2"], var.app_service_sku)
    error_message = "App Service SKU must be a valid Azure App Service Plan tier"
  }
}

# Storage Account configuration for OneDrive integration
variable "storage_account_tier" {
  type        = string
  description = "Performance tier for Azure Storage Account (Standard or Premium)"
  default     = "Standard"
  validation {
    condition     = contains(["Standard", "Premium"], title(var.storage_account_tier))
    error_message = "Storage Account tier must be either Standard or Premium"
  }
}

# Key Vault configuration for secrets management
variable "key_vault_sku" {
  type        = string
  description = "SKU for Azure Key Vault (determines pricing tier and features)"
  default     = "standard"
  validation {
    condition     = contains(["standard", "premium"], lower(var.key_vault_sku))
    error_message = "Key Vault SKU must be either standard or premium"
  }
}

# Container Registry configuration
variable "container_registry_sku" {
  type        = string
  description = "SKU for Azure Container Registry (determines pricing tier and features)"
  default     = "Standard"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], title(var.container_registry_sku))
    error_message = "Container Registry SKU must be Basic, Standard, or Premium"
  }
}