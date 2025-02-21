#!/bin/bash
set -euo pipefail

# Azure Infrastructure Deployment Script
# Version: 1.0.0
# Description: Orchestrates secure deployment of complete infrastructure stack

# Global variables
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
LOG_DIR="/var/log/deployments"
LOG_FILE="${LOG_DIR}/deployment-${TIMESTAMP}.log"
BICEP_DIR="${SCRIPT_DIR}/../azure"

# Required environment variables
declare -a REQUIRED_ENV=(
    "AZURE_SUBSCRIPTION"
    "AZURE_TENANT"
    "ENV_NAME"
)

# Logging setup
setup_logging() {
    mkdir -p "${LOG_DIR}"
    exec 1> >(tee -a "${LOG_FILE}")
    exec 2> >(tee -a "${LOG_FILE}" >&2)
    echo "[INFO] Deployment started at $(date)"
}

# Error handling
cleanup_on_error() {
    local exit_code=$?
    echo "[ERROR] Deployment failed with exit code ${exit_code}"
    echo "[INFO] Cleaning up resources..."
    # Add cleanup logic here if needed
    exit "${exit_code}"
}

trap cleanup_on_error ERR INT TERM EXIT

# Function to log messages
log() {
    local level=$1
    shift
    echo "[${level}] $(date '+%Y-%m-%d %H:%M:%S'): $*"
}

# Check prerequisites
check_prerequisites() {
    log "INFO" "Checking prerequisites..."

    # Check required environment variables
    for env_var in "${REQUIRED_ENV[@]}"; do
        if [[ -z "${!env_var:-}" ]]; then
            log "ERROR" "Required environment variable ${env_var} is not set"
            return 1
        fi
    done

    # Check Azure CLI version
    local az_version
    az_version=$(az version --query '"azure-cli"' -o tsv)
    if [[ $(echo "${az_version} 2.50.0" | tr " " "\n" | sort -V | head -n 1) != "2.50.0" ]]; then
        log "ERROR" "Azure CLI version must be >= 2.50.0. Current version: ${az_version}"
        return 1
    fi

    # Check kubectl version
    if ! command -v kubectl &> /dev/null; then
        log "ERROR" "kubectl is not installed"
        return 1
    fi

    # Check helm version
    if ! command -v helm &> /dev/null; then
        log "ERROR" "helm is not installed"
        return 1
    fi

    # Verify Azure login
    if ! az account show &> /dev/null; then
        log "ERROR" "Not logged into Azure. Please run 'az login'"
        return 1
    }

    # Check Bicep templates
    if [[ ! -f "${BICEP_DIR}/main.bicep" ]]; then
        log "ERROR" "Required Bicep template not found: ${BICEP_DIR}/main.bicep"
        return 1
    }

    log "INFO" "Prerequisites check completed successfully"
    return 0
}

# Deploy Azure infrastructure
deploy_azure_infrastructure() {
    local environment=$1
    log "INFO" "Deploying Azure infrastructure for environment: ${environment}"

    # Select parameter file based on environment
    local param_file="${BICEP_DIR}/parameters/${environment}.parameters.json"
    if [[ ! -f "${param_file}" ]]; then
        log "ERROR" "Parameter file not found: ${param_file}"
        return 1
    }

    # Deploy infrastructure using Bicep
    az deployment sub create \
        --name "deployment-${TIMESTAMP}" \
        --location eastus \
        --template-file "${BICEP_DIR}/main.bicep" \
        --parameters "@${param_file}" \
        --parameters environment="${environment}" \
        || return 1

    log "INFO" "Azure infrastructure deployment completed"
    return 0
}

# Configure Kubernetes
configure_kubernetes() {
    log "INFO" "Configuring Kubernetes cluster"

    # Get AKS credentials
    az aks get-credentials \
        --resource-group "${ENV_NAME}-rg" \
        --name "${ENV_NAME}-aks" \
        --overwrite-existing

    # Apply network policies
    kubectl apply -f "${SCRIPT_DIR}/k8s/network-policies.yaml"

    # Configure namespaces and quotas
    kubectl apply -f "${SCRIPT_DIR}/k8s/namespaces.yaml"
    kubectl apply -f "${SCRIPT_DIR}/k8s/resource-quotas.yaml"

    # Setup RBAC
    kubectl apply -f "${SCRIPT_DIR}/k8s/rbac.yaml"

    # Install cert-manager
    helm repo add jetstack https://charts.jetstack.io
    helm repo update
    helm upgrade --install \
        cert-manager jetstack/cert-manager \
        --namespace cert-manager \
        --create-namespace \
        --version v1.12.0 \
        --set installCRDs=true

    log "INFO" "Kubernetes configuration completed"
    return 0
}

# Deploy applications
deploy_applications() {
    log "INFO" "Deploying applications"

    # Deploy backend services
    kubectl apply -f "${SCRIPT_DIR}/k8s/backend-deployment.yaml"
    kubectl apply -f "${SCRIPT_DIR}/k8s/backend-service.yaml"

    # Deploy frontend application
    kubectl apply -f "${SCRIPT_DIR}/k8s/frontend-deployment.yaml"
    kubectl apply -f "${SCRIPT_DIR}/k8s/frontend-service.yaml"

    # Configure ingress
    kubectl apply -f "${SCRIPT_DIR}/k8s/ingress.yaml"

    log "INFO" "Application deployment completed"
    return 0
}

# Setup monitoring
setup_monitoring() {
    log "INFO" "Setting up monitoring stack"

    # Add Helm repositories
    helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
    helm repo add grafana https://grafana.github.io/helm-charts
    helm repo update

    # Install Prometheus
    helm upgrade --install \
        prometheus prometheus-community/prometheus \
        --namespace monitoring \
        --create-namespace \
        --values "${SCRIPT_DIR}/monitoring/prometheus-values.yaml"

    # Install Grafana
    helm upgrade --install \
        grafana grafana/grafana \
        --namespace monitoring \
        --values "${SCRIPT_DIR}/monitoring/grafana-values.yaml"

    log "INFO" "Monitoring setup completed"
    return 0
}

# Main function
main() {
    local environment=${1:-}

    if [[ -z "${environment}" ]]; then
        log "ERROR" "Environment parameter is required"
        return 1
    }

    # Setup logging
    setup_logging

    # Execute deployment steps
    check_prerequisites || return 1
    deploy_azure_infrastructure "${environment}" || return 1
    configure_kubernetes || return 1
    deploy_applications || return 1
    setup_monitoring || return 1

    log "INFO" "Deployment completed successfully"
    return 0
}

# Execute main function with environment parameter
main "$@"