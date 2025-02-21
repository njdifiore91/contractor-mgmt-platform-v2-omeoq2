# Environment identifier
environment = "dev"

# Resource group configuration
resource_group_name = "rg-inspector-dev-eastus"
location = "eastus"

# App Service Plan configuration for web application hosting
app_service_plan_tier = "Standard"
app_service_plan_size = "S1"

# SQL Server configuration for database hosting
sql_server_name = "sql-inspector-dev-eastus"
sql_database_name = "sqldb-inspector-dev"
sql_admin_login = "sqladmin"

# Key Vault configuration for secrets management
key_vault_name = "kv-inspector-dev-eastus"
key_vault_sku = "standard"

# Container Registry configuration for Docker images
container_registry_name = "acrinspectordev"
container_registry_sku = "Basic"

# Storage Account configuration for OneDrive integration
storage_account_name = "stinspectordev"
storage_account_tier = "Standard"

# Resource tagging for cost allocation and management
tags = {
  Environment         = "Development"
  Project            = "Inspector Management System"
  CostCenter         = "Development"
  ManagedBy          = "Terraform"
  DataClassification = "Internal"
  BusinessUnit       = "Engineering"
}