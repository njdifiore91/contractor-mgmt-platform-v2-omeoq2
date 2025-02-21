# Azure Provider version ~> 3.0
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# Variables
variable "resource_group_name" {
  type        = string
  description = "Name of the resource group where App Service resources will be created"
}

variable "location" {
  type        = string
  description = "Azure region where resources will be deployed"
}

variable "environment" {
  type        = string
  description = "Environment name (dev, prod) for resource naming and tagging"
}

variable "app_service_sku" {
  type = object({
    tier = string
    size = string
  })
  description = "SKU configuration for App Service Plan including tier and size"
}

variable "retention_days" {
  type        = number
  description = "Retention period in days for Application Insights data"
  default     = 90
}

variable "allowed_origins" {
  type        = list(string)
  description = "List of allowed origins for CORS configuration"
}

# Local variables for naming convention
locals {
  app_service_plan_name = "asp-${var.environment}"
  backend_app_name      = "app-backend-${var.environment}"
  frontend_app_name     = "app-frontend-${var.environment}"
  app_insights_name     = "appi-${var.environment}"
  common_tags = {
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# App Service Plan
resource "azurerm_app_service_plan" "main" {
  name                = local.app_service_plan_name
  location            = var.location
  resource_group_name = var.resource_group_name
  kind                = "Windows"

  sku {
    tier = var.app_service_sku.tier
    size = var.app_service_sku.size
  }

  zone_redundant    = true
  per_site_scaling  = true
  tags              = local.common_tags
}

# Backend App Service (ASP.NET Core API)
resource "azurerm_app_service" "backend" {
  name                = local.backend_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  app_service_plan_id = azurerm_app_service_plan.main.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on           = true
    http2_enabled       = true
    min_tls_version    = "1.2"
    ftps_state         = "Disabled"
    health_check_path  = "/health"

    cors {
      allowed_origins     = var.allowed_origins
      support_credentials = true
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"             = azurerm_application_insights.main.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"       = azurerm_application_insights.main.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~2"
    "WEBSITE_RUN_FROM_PACKAGE"                   = "1"
    "ASPNETCORE_ENVIRONMENT"                     = var.environment
  }

  backup {
    enabled = true
    name    = "backup"
    schedule {
      frequency_interval = 1
      frequency_unit    = "Day"
      retention_period_days = 30
    }
  }

  tags = local.common_tags
}

# Frontend App Service (Vue.js)
resource "azurerm_app_service" "frontend" {
  name                = local.frontend_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  app_service_plan_id = azurerm_app_service_plan.main.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on          = true
    http2_enabled      = true
    min_tls_version    = "1.2"
    ftps_state         = "Disabled"
    websockets_enabled = true

    cors {
      allowed_origins     = var.allowed_origins
      support_credentials = true
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"             = azurerm_application_insights.main.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"       = azurerm_application_insights.main.connection_string
    "WEBSITE_NODE_DEFAULT_VERSION"               = "~16"
    "WEBSITE_RUN_FROM_PACKAGE"                   = "1"
  }

  tags = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = local.app_insights_name
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  workspace_mode      = "Enabled"
  retention_in_days   = var.retention_days
  sampling_percentage = 100
  disable_ip_masking  = false

  tags = local.common_tags
}

# Outputs
output "app_service_plan" {
  value = {
    id                           = azurerm_app_service_plan.main.id
    name                         = azurerm_app_service_plan.main.name
    maximum_elastic_worker_count = azurerm_app_service_plan.main.maximum_elastic_worker_count
  }
  description = "App Service Plan resource for hosting web apps with scaling information"
}

output "backend_app_service" {
  value = {
    id               = azurerm_app_service.backend.id
    name             = azurerm_app_service.backend.name
    default_hostname = azurerm_app_service.backend.default_site_hostname
    identity         = azurerm_app_service.backend.identity
  }
  description = "App Service instance for ASP.NET Core backend API with security configurations"
}

output "frontend_app_service" {
  value = {
    id               = azurerm_app_service.frontend.id
    name             = azurerm_app_service.frontend.name
    default_hostname = azurerm_app_service.frontend.default_site_hostname
    identity         = azurerm_app_service.frontend.identity
  }
  description = "App Service instance for Vue.js frontend application with security configurations"
}

output "app_insights" {
  value = {
    id                  = azurerm_application_insights.main.id
    instrumentation_key = azurerm_application_insights.main.instrumentation_key
    connection_string   = azurerm_application_insights.main.connection_string
  }
  description = "Application Insights resource for monitoring and telemetry with enhanced configuration"
}