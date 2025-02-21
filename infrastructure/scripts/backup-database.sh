#!/bin/bash

# Database Backup Script for SQL Server with Azure Integration
# Version: 1.0.0
# Dependencies:
# - azure-cli v2.x
# - mssql-tools v17.x

set -euo pipefail
IFS=$'\n\t'

# Global variables with default values
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/tmp/backups"
LOG_FILE="/var/log/database-backup.log"
RETENTION_DAYS=30
MAX_RETRIES=3
RETRY_DELAY=60
COMPRESSION_LEVEL=9
AZURE_STORAGE_CONTAINER="database-backups"
KEYVAULT_NAME="backup-credentials"
METRICS_FILE="/var/log/backup-metrics.log"
CORRELATION_ID=$(uuidgen)

# Logging function with severity levels and structured output
log_message() {
    local message=$1
    local severity=${2:-INFO}
    local context=${3:-MAIN}
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    local log_entry="[$timestamp][$severity][$context][CorrelationId:$CORRELATION_ID] $message"
    
    echo "$log_entry" | tee -a "$LOG_FILE"
    
    if [[ "$severity" == "ERROR" || "$severity" == "FATAL" ]]; then
        echo "$(date -u +"%Y-%m-%dT%H:%M:%SZ"),$severity,$context,$message" >> "$METRICS_FILE"
    fi
}

# Input validation function
validate_inputs() {
    local backup_type=$1
    local database_name=$2
    local retention_period=$3

    # Validate backup type
    if [[ ! "$backup_type" =~ ^(FULL|DIFFERENTIAL)$ ]]; then
        log_message "Invalid backup type: $backup_type" "ERROR" "VALIDATION"
        return 1
    fi

    # Validate database name (prevent SQL injection)
    if [[ ! "$database_name" =~ ^[a-zA-Z0-9_-]+$ ]]; then
        log_message "Invalid database name: $database_name" "ERROR" "VALIDATION"
        return 1
    fi

    # Validate retention period
    if ! [[ "$retention_period" =~ ^[0-9]+$ ]] || [ "$retention_period" -lt 1 ] || [ "$retention_period" -gt 365 ]; then
        log_message "Invalid retention period: $retention_period" "ERROR" "VALIDATION"
        return 1
    fi

    # Check required environment variables
    local required_vars=("AZURE_SUBSCRIPTION_ID" "AZURE_TENANT_ID" "SQL_SERVER_NAME")
    for var in "${required_vars[@]}"; do
        if [ -z "${!var:-}" ]; then
            log_message "Missing required environment variable: $var" "ERROR" "VALIDATION"
            return 1
        fi
    done

    # Verify backup directory exists and is writable
    if [ ! -d "$BACKUP_DIR" ] || [ ! -w "$BACKUP_DIR" ]; then
        log_message "Backup directory not accessible: $BACKUP_DIR" "ERROR" "VALIDATION"
        return 1
    fi

    return 0
}

# Secure credential retrieval from Azure Key Vault
get_credentials() {
    local credential_type=$1
    local credential=""
    local retry_count=0

    while [ $retry_count -lt $MAX_RETRIES ]; do
        try {
            # Authenticate to Azure
            az login --identity --allow-no-subscriptions > /dev/null 2>&1
            
            # Retrieve credential from Key Vault
            credential=$(az keyvault secret show \
                --vault-name "$KEYVAULT_NAME" \
                --name "$credential_type" \
                --query "value" \
                --output tsv)

            if [ -n "$credential" ]; then
                break
            fi
        } catch {
            retry_count=$((retry_count + 1))
            log_message "Failed to retrieve credential: $credential_type. Attempt $retry_count of $MAX_RETRIES" "WARN" "CREDENTIALS"
            sleep $((RETRY_DELAY * retry_count))
        }
    done

    if [ -z "$credential" ]; then
        log_message "Failed to retrieve credential after $MAX_RETRIES attempts" "FATAL" "CREDENTIALS"
        exit 1
    fi

    echo "$credential"
}

# Perform database backup
perform_backup() {
    local backup_type=$1
    local database_name=$2
    local start_time=$(date +%s)
    local backup_file="${BACKUP_DIR}/${database_name}_${backup_type}_${TIMESTAMP}.bak"
    local backup_log="${BACKUP_DIR}/${database_name}_${backup_type}_${TIMESTAMP}.log"
    local checksum_file="${backup_file}.sha256"
    local retry_count=0

    # Get database credentials
    local sql_password=$(get_credentials "sql-password")
    local sql_user=$(get_credentials "sql-user")

    while [ $retry_count -lt $MAX_RETRIES ]; do
        try {
            # Execute backup
            sqlcmd -S "$SQL_SERVER_NAME" \
                   -U "$sql_user" \
                   -P "$sql_password" \
                   -Q "BACKUP DATABASE [$database_name] TO DISK='$backup_file' \
                       WITH FORMAT, COMPRESSION, CHECKSUM, \
                       STATS=10, \
                       NAME='$database_name-$backup_type-$TIMESTAMP'" \
                   > "$backup_log" 2>&1

            # Verify backup
            if ! sqlcmd -S "$SQL_SERVER_NAME" \
                       -U "$sql_user" \
                       -P "$sql_password" \
                       -Q "RESTORE VERIFYONLY FROM DISK='$backup_file'"; then
                throw "Backup verification failed"
            fi

            # Calculate checksum
            sha256sum "$backup_file" > "$checksum_file"

            # Upload to Azure Storage
            local storage_account=$(get_credentials "storage-account")
            local storage_key=$(get_credentials "storage-key")

            az storage blob upload \
                --account-name "$storage_account" \
                --account-key "$storage_key" \
                --container-name "$AZURE_STORAGE_CONTAINER" \
                --file "$backup_file" \
                --name "$(basename "$backup_file")" \
                --tier "Cool"

            # Upload checksum
            az storage blob upload \
                --account-name "$storage_account" \
                --account-key "$storage_key" \
                --container-name "$AZURE_STORAGE_CONTAINER" \
                --file "$checksum_file" \
                --name "$(basename "$checksum_file")" \
                --tier "Cool"

            break
        } catch {
            retry_count=$((retry_count + 1))
            log_message "Backup attempt $retry_count failed" "WARN" "BACKUP"
            sleep $((RETRY_DELAY * retry_count))
        }
    done

    if [ $retry_count -eq $MAX_RETRIES ]; then
        log_message "Backup failed after $MAX_RETRIES attempts" "FATAL" "BACKUP"
        exit 1
    fi

    # Calculate metrics
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    local backup_size=$(stat -f %z "$backup_file")

    # Log metrics
    echo "$(date -u +"%Y-%m-%dT%H:%M:%SZ"),BACKUP,$database_name,$backup_type,$duration,$backup_size" >> "$METRICS_FILE"

    # Cleanup local files
    rm -f "$backup_file" "$backup_log" "$checksum_file"

    return 0
}

# Main execution
main() {
    local backup_type=${1:-FULL}
    local database_name=${2:-}
    local retention_period=${3:-$RETENTION_DAYS}

    log_message "Starting backup process" "INFO" "MAIN"

    # Validate inputs
    if ! validate_inputs "$backup_type" "$database_name" "$retention_period"; then
        log_message "Input validation failed" "FATAL" "MAIN"
        exit 1
    fi

    # Perform backup
    if ! perform_backup "$backup_type" "$database_name"; then
        log_message "Backup process failed" "FATAL" "MAIN"
        exit 1
    fi

    # Cleanup old backups from Azure Storage
    local storage_account=$(get_credentials "storage-account")
    local storage_key=$(get_credentials "storage-key")

    az storage blob delete-batch \
        --account-name "$storage_account" \
        --account-key "$storage_key" \
        --source "$AZURE_STORAGE_CONTAINER" \
        --if-unmodified-since "$(date -d "-$retention_period days" -u +"%Y-%m-%dT%H:%M:%SZ")"

    log_message "Backup process completed successfully" "INFO" "MAIN"
}

# Trap errors
trap 'log_message "Script error on line $LINENO" "ERROR" "TRAP"' ERR

# Execute main function with command line arguments
main "$@"