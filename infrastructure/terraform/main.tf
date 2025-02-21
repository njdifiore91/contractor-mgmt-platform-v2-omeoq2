# Provider and backend configuration
terraform {
  required_version = ">= 1.0.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
    sendgrid = {
      source  = "registry.terraform.io/providers/sendgrid/sendgrid"
      version = "~> 1.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "${var.resource_name_prefix}-tfstate"
    storage_account_name = "${var.resource_name_prefix}tfstate"
    container_name      = "tfstate"
    key                 = "${var.environment}.terraform.tfstate"
  }
}

# Configure providers
provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted_items   = true
    }
  }
}

provider "random" {}
provider "sendgrid" {}

# Local values for resource naming and tagging
locals {
  common_tags = {
    Environment   = var.environment
    ManagedBy    = "Terraform"
    CostCenter   = "IT-Infrastructure"
    BackupPolicy = var.environment == "prod" ? "Standard" : "Basic"
  }

  resource_prefix = "${var.resource_name_prefix}-${var.environment}"
  
  backup_retention = {
    dev     = 7
    staging = 14
    prod    = 30
  }

  network_rules = {
    inbound_rules = [
      {
        name                   = "allow-https"
        priority              = 100
        direction             = "Inbound"
        access                = "Allow"
        protocol              = "Tcp"
        source_port_range     = "*"
        destination_port_range = "443"
        source_address_prefix = "Internet"
      }
    ]
  }
}

# Random string for unique resource names
resource "random_string" "unique" {
  length  = 8
  special = false
  upper   = false
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${local.resource_prefix}-rg"
  location = var.location
  tags     = local.common_tags
}

# App Service Module
module "app_service" {
  source = "./modules/app-service"
  
  resource_group_name = azurerm_resource_group.main.name
  location           = azurerm_resource_group.main.location
  resource_prefix    = local.resource_prefix
  app_service_sku    = var.app_service_sku
  common_tags        = local.common_tags
}

# SQL Server Module
module "sql_server" {
  source = "./modules/sql-server"
  
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  resource_prefix           = local.resource_prefix
  admin_username            = var.sql_server_admin_username
  admin_password            = var.sql_server_admin_password
  backup_retention_days     = local.backup_retention[var.environment]
  common_tags               = local.common_tags
}

# Storage Account Module
module "storage" {
  source = "./modules/storage"
  
  resource_group_name    = azurerm_resource_group.main.name
  location              = azurerm_resource_group.main.location
  resource_prefix       = local.resource_prefix
  account_tier          = var.storage_account_tier
  common_tags           = local.common_tags
}

# Key Vault Module
module "key_vault" {
  source = "./modules/key-vault"
  
  resource_group_name = azurerm_resource_group.main.name
  location           = azurerm_resource_group.main.location
  resource_prefix    = local.resource_prefix
  sku_name           = var.key_vault_sku
  common_tags        = local.common_tags
}

# Container Registry Module
module "container_registry" {
  source = "./modules/container-registry"
  
  resource_group_name = azurerm_resource_group.main.name
  location           = azurerm_resource_group.main.location
  resource_prefix    = local.resource_prefix
  sku               = var.container_registry_sku
  common_tags        = local.common_tags
}

# SendGrid Module
module "sendgrid" {
  source = "./modules/sendgrid"
  
  resource_group_name = azurerm_resource_group.main.name
  resource_prefix    = local.resource_prefix
  common_tags        = local.common_tags
}

# Monitoring Module
module "monitoring" {
  source = "./modules/monitoring"
  
  resource_group_name = azurerm_resource_group.main.name
  location           = azurerm_resource_group.main.location
  resource_prefix    = local.resource_prefix
  common_tags        = local.common_tags
}

# Network Security Module
module "network_security" {
  source = "./modules/network-security"
  
  resource_group_name = azurerm_resource_group.main.name
  location           = azurerm_resource_group.main.location
  resource_prefix    = local.resource_prefix
  network_rules      = local.network_rules
  common_tags        = local.common_tags
}

# Outputs
output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "web_app_url" {
  value = module.app_service.web_app_url
}

output "api_app_url" {
  value = module.app_service.api_app_url
}

output "sql_server_name" {
  value = module.sql_server.server_name
}

output "sql_database_name" {
  value = module.sql_server.database_name
}

output "storage_account_name" {
  value = module.storage.storage_account_name
}

output "key_vault_name" {
  value = module.key_vault.key_vault_name
}

output "container_registry_name" {
  value = module.container_registry.registry_name
}

output "container_registry_login_server" {
  value = module.container_registry.registry_login_server
}

output "monitoring_workspace_id" {
  value = module.monitoring.workspace_id
}

output "network_security_group_id" {
  value = module.network_security.nsg_id
}