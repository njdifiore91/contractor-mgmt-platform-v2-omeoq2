# Infrastructure Documentation

## Infrastructure Overview

This document outlines the cloud infrastructure architecture for the inspector management application. The infrastructure is built on Microsoft Azure and encompasses the following key components:

- Azure Kubernetes Service (AKS) for application hosting
- Azure SQL Server for geographical search and data storage
- Azure Communication Services for email functionality
- Microsoft Graph API integration for OneDrive management
- Azure Monitor and Application Insights for monitoring
- Azure Key Vault for secrets management

### Architecture Diagram

```ascii
+------------------------+
|     Azure Front Door   |
+------------------------+
           ↓
+------------------------+
|   Azure Kubernetes    |
|      Service (AKS)    |
+------------------------+
           ↓
+------------------------+
|   Azure Services      |
|   - SQL Server       |
|   - Key Vault        |
|   - Communication    |
|   - Storage Account  |
+------------------------+
```

## Prerequisites

### Required Permissions

#### Azure AD Roles
- Global Administrator or Application Administrator
- Azure Subscription Owner/Contributor
- Azure AD Directory Reader

#### Service Principal Requirements
- AKS Cluster Admin
- Key Vault Secrets Officer
- Storage Blob Data Contributor
- SQL Server Contributor

### Tools and Versions

Required tools and minimum versions:
- Azure CLI (2.40.0+)
- kubectl (1.24.0+)
- Bicep CLI (0.9.0+)
- Azure PowerShell (7.0.0+)

## Azure Resources

### Resource Templates

Our infrastructure is defined using Bicep templates located in `infrastructure/azure/`. Key components include:

- Virtual Network and Subnets
- AKS Cluster
- Azure SQL Server
- Azure Communication Services
- Azure Key Vault
- Azure Storage Account
- Azure Monitor

### Environment Parameters

Environment-specific configurations are maintained in JSON parameter files:
- Development: `infrastructure/azure/parameters/dev.parameters.json`
- Staging: `infrastructure/azure/parameters/staging.parameters.json`
- Production: `infrastructure/azure/parameters/prod.parameters.json`

### Security Configuration

- Network Security Groups (NSGs) with restricted inbound/outbound rules
- Private endpoints for Azure services
- Azure AD integration for authentication
- TLS/SSL encryption for all endpoints
- Role-Based Access Control (RBAC)

## Service Integrations

### OneDrive Setup

Configuration for automatic inspector folder management:

1. Microsoft Graph API registration
2. Required permissions:
   - Files.ReadWrite.All
   - Sites.ReadWrite.All
3. Folder structure:
   ```
   /Inspectors
   └── {Inspector_ID}
       ├── Documents
       ├── Certifications
       └── Records
   ```

### Email Service

Azure Communication Services configuration:

1. Email service provisioning
2. Template management
3. SMTP relay setup
4. Attachment handling configuration

### Geographical Search

SQL Server geographical search setup:

1. Spatial data types configuration
2. Indexing strategy
3. Performance optimization
4. Query templates

## Monitoring and Operations

### Monitoring Tools

- Azure Monitor for infrastructure metrics
- Application Insights for application telemetry
- Log Analytics for centralized logging
- Custom dashboards for key metrics

### Backup and Recovery

- SQL Database: Geo-redundant backups
- Storage Account: RA-GRS replication
- AKS: Control plane backup
- Recovery Time Objective (RTO): 4 hours
- Recovery Point Objective (RPO): 1 hour

## Security and Compliance

- Regular security assessments
- Compliance monitoring
- Access reviews (quarterly)
- Security patch management
- Encryption at rest and in transit
- Audit logging

## Troubleshooting

### Common Issues

1. AKS Connectivity
   ```bash
   az aks get-credentials --resource-group <rg-name> --name <cluster-name>
   kubectl get nodes
   ```

2. SQL Connectivity
   ```bash
   Test-NetConnection -ComputerName <server>.database.windows.net -Port 1433
   ```

3. Email Service
   ```bash
   az communication email domain list
   az communication email test
   ```

### Support Escalation

1. L1: Infrastructure Team
2. L2: Cloud Platform Team
3. L3: Microsoft Support

## Contributing

1. Fork the infrastructure repository
2. Create a feature branch
3. Submit a pull request with:
   - Detailed description
   - Testing results
   - Security review results
   - Compliance check results

---

For additional support, contact the infrastructure team at infrastructure@company.com