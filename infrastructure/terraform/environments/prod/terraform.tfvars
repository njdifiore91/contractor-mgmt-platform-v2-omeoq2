# Production Resource Group
resource_group_name = "inspector-mgmt-prod-rg"

# App Service Configuration
app_service_plan_name = "inspector-mgmt-prod-asp"
app_service_sku = "P1v2"  # Premium v2 tier for production workloads
app_service_instance_count = 3  # High availability with multiple instances

# SQL Server Configuration
sql_server_name = "inspector-mgmt-prod-sql"
sql_database_name = "InspectorMgmtProd"
sql_database_sku = "S1"  # Production performance tier
sql_geo_replica_location = "westus2"  # Secondary region for disaster recovery

# Key Vault Configuration
key_vault_name = "inspector-mgmt-prod-kv"
key_vault_sku = "Premium"  # Enhanced security features for production

# Container Registry Configuration
container_registry_name = "inspectormgmtprodacr"
container_registry_sku = "Premium"  # Production grade with geo-replication

# Storage Account Configuration for Inspector Files
storage_account_name = "inspectormgmtprodst"
storage_account_tier = "Standard"
storage_account_replication = "GRS"  # Geo-redundant storage for data protection

# Network Security Configuration
ddos_protection_plan = true  # Enhanced DDoS protection for production

# Global Load Balancing
front_door_name = "inspector-mgmt-prod-fd"