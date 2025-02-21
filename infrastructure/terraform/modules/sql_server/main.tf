# Provider configuration requirements
# Azure RM Provider v3.0+ for Azure resource management
# Random Provider v3.0+ for generating secure passwords
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

# Input variables
variable "resource_group_name" {
  type        = string
  description = "Name of the resource group where SQL Server will be deployed"
}

variable "location" {
  type        = string
  description = "Azure region where SQL Server will be deployed"
}

variable "environment" {
  type        = string
  description = "Environment name (dev, prod) for resource naming and tagging"
}

variable "tags" {
  type        = map(string)
  description = "Tags to be applied to all resources"
}

variable "enable_ad_auth" {
  type        = bool
  default     = true
  description = "Enable Azure AD authentication"
}

variable "backup_retention_days" {
  type        = number
  default     = 7
  description = "Number of days to retain backups"
}

# Generate secure random password for SQL admin
resource "random_password" "sql_admin" {
  length           = 16
  special          = true
  min_special      = 2
  min_numeric      = 2
  min_upper        = 2
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

# SQL Server instance
resource "azurerm_mssql_server" "sql_server" {
  name                         = "${lower(replace(var.environment, "-", ""))}-sql"
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = random_password.sql_admin.result
  minimum_tls_version         = "1.2"
  public_network_access_enabled = true

  # Azure AD authentication configuration
  dynamic "azuread_administrator" {
    for_each = var.enable_ad_auth ? [1] : []
    content {
      login_username = "SQL Admin Group"
      object_id     = data.azurerm_client_config.current.object_id
      tenant_id     = data.azurerm_client_config.current.tenant_id
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Application database
resource "azurerm_mssql_database" "app_db" {
  name                        = "${lower(replace(var.environment, "-", ""))}-db"
  server_id                   = azurerm_mssql_server.sql_server.id
  collation                   = "SQL_Latin1_General_CP1_CI_AS"
  sku_name                    = "S1"
  max_size_gb                 = 50
  zone_redundant             = true
  auto_pause_delay_in_minutes = 60

  short_term_retention_policy {
    retention_days = var.backup_retention_days
  }

  long_term_retention_policy {
    weekly_retention  = "P4W"
    monthly_retention = "P12M"
  }

  storage_account_type = "Geo"

  # Enable Query Store for performance monitoring
  query_store_enabled = true

  tags = var.tags
}

# Allow Azure services to access SQL Server
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "allow-azure-services"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Diagnostic settings for monitoring
resource "azurerm_monitor_diagnostic_setting" "sql_diagnostics" {
  name                       = "sql-diagnostics"
  target_resource_id        = azurerm_mssql_server.sql_server.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.workspace.id

  dynamic "enabled_log" {
    for_each = ["SQLSecurityAuditEvents", "SQLInsights", "AutomaticTuning", "QueryStoreRuntimeStatistics", "QueryStoreWaitStatistics", "Errors"]
    content {
      category = enabled_log.value
    }
  }

  metric {
    category = "Basic"
    enabled  = true
  }

  metric {
    category = "InstanceAndAppAdvanced"
    enabled  = true
  }

  metric {
    category = "WorkloadManagement"
    enabled  = true
  }
}

# Data source for current Azure context
data "azurerm_client_config" "current" {}

# Outputs
output "sql_server_name" {
  value       = azurerm_mssql_server.sql_server.name
  description = "The name of the provisioned SQL Server instance"
}

output "sql_server_fqdn" {
  value       = azurerm_mssql_server.sql_server.fully_qualified_domain_name
  description = "The fully qualified domain name of the SQL Server"
}

output "database_name" {
  value       = azurerm_mssql_database.app_db.name
  description = "The name of the created application database"
}

output "sql_admin_login" {
  value       = azurerm_mssql_server.sql_server.administrator_login
  description = "The administrator login name for SQL Server"
}

output "sql_admin_password" {
  value       = random_password.sql_admin.result
  sensitive   = true
  description = "The administrator password for SQL Server (sensitive value)"
}