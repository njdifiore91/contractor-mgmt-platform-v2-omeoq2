#!/bin/bash

# Infrastructure Initialization Script
# Version: 1.0.0
# Description: Sets up complete infrastructure stack for Inspector Management System

set -euo pipefail
IFS=$'\n\t'

# Function to check all prerequisites
check_prerequisites() {
    echo "Checking prerequisites..."
    
    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        echo "Error: Azure CLI not found"
        return 1
    fi
    
    # Verify Azure login
    if ! az account show &> /dev/null; then
        echo "Error: Not logged into Azure"
        return 1
    }
    
    # Check Terraform
    if ! command -v terraform &> /dev/null; then
        echo "Error: Terraform not found"
        return 1
    }
    
    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        echo "Error: kubectl not found"
        return 1
    }
    
    # Verify required environment variables
    required_vars=("AZURE_SUBSCRIPTION_ID" "AZURE_TENANT_ID" "AZURE_CLIENT_ID" "AZURE_CLIENT_SECRET")
    for var in "${required_vars[@]}"; do
        if [[ -z "${!var:-}" ]]; then
            echo "Error: Required environment variable $var not set"
            return 1
        fi
    done
    
    return 0
}

# Function to initialize Azure resources
init_azure_resources() {
    local environment=$1
    echo "Initializing Azure resources for environment: $environment"
    
    # Deploy Bicep template
    az deployment sub create \
        --name "inspector-system-$environment" \
        --location eastus \
        --template-file ../azure/main.bicep \
        --parameters environment=$environment \
        --parameters projectName=inspector \
        --parameters location=eastus \
        || return 1
    
    echo "Azure resources initialized successfully"
    return 0
}

# Function to initialize Terraform resources
init_terraform_resources() {
    local environment=$1
    echo "Initializing Terraform resources for environment: $environment"
    
    # Navigate to Terraform directory
    cd ../terraform
    
    # Initialize Terraform
    terraform init || return 1
    
    # Apply Terraform configuration
    terraform apply \
        -var="environment=$environment" \
        -var="location=eastus" \
        -var="resource_name_prefix=inspector" \
        -auto-approve \
        || return 1
    
    echo "Terraform resources initialized successfully"
    return 0
}

# Function to setup Kubernetes cluster
setup_kubernetes() {
    echo "Setting up Kubernetes resources"
    
    # Create namespaces
    kubectl create namespace production --dry-run=client -o yaml | kubectl apply -f -
    
    # Apply configurations
    kubectl apply -f ../kubernetes/config-maps.yaml
    kubectl apply -f ../kubernetes/secrets.yaml
    kubectl apply -f ../kubernetes/backend-deployment.yaml
    kubectl apply -f ../kubernetes/backend-service.yaml
    kubectl apply -f ../kubernetes/web-deployment.yaml
    
    echo "Kubernetes resources setup successfully"
    return 0
}

# Function to setup monitoring
setup_monitoring() {
    echo "Setting up monitoring infrastructure"
    
    # Deploy Azure Monitor resources
    az monitor workspace create \
        --resource-group "inspector-${environment}-rg" \
        --workspace-name "inspector-${environment}-workspace" \
        || return 1
    
    # Setup diagnostic settings
    az monitor diagnostic-settings create \
        --resource "inspector-${environment}-sql" \
        --name "inspector-${environment}-diagnostics" \
        --workspace "inspector-${environment}-workspace" \
        || return 1
    
    echo "Monitoring infrastructure setup successfully"
    return 0
}

# Main function
main() {
    local environment=${1:-}
    
    # Validate environment parameter
    if [[ ! $environment =~ ^(dev|staging|prod)$ ]]; then
        echo "Error: Invalid environment. Must be dev, staging, or prod"
        return 1
    }
    
    # Initialize logging
    exec 1> >(tee -a "infrastructure-init-${environment}.log")
    exec 2>&1
    
    echo "Starting infrastructure initialization for environment: $environment"
    
    # Execute initialization steps
    check_prerequisites || { echo "Prerequisites check failed"; return 1; }
    init_azure_resources "$environment" || { echo "Azure initialization failed"; return 1; }
    init_terraform_resources "$environment" || { echo "Terraform initialization failed"; return 1; }
    setup_kubernetes || { echo "Kubernetes setup failed"; return 1; }
    setup_monitoring || { echo "Monitoring setup failed"; return 1; }
    
    echo "Infrastructure initialization completed successfully"
    return 0
}

# Script entry point
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
```

This script implements a comprehensive infrastructure initialization process with:

1. Prerequisite checking for all required tools and credentials
2. Azure resource deployment using Bicep templates
3. Additional infrastructure setup using Terraform
4. Kubernetes cluster configuration and deployment
5. Monitoring infrastructure setup
6. Robust error handling and logging
7. Environment-specific configuration support

The script follows enterprise-grade practices including:
- Strict error handling with set -euo pipefail
- Comprehensive logging
- Environment validation
- Modular function design
- Security-first approach with required authentication checks
- Idempotent operations where possible

The script is designed to be run as:
```bash
./init-infrastructure.sh <environment>