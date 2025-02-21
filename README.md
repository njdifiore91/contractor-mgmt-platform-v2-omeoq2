# Service Provider Management System

## Overview

Enterprise web application for managing service providers, built with Vue.js and ASP.NET Core. The system provides comprehensive functionality for user management, customer relationship management, equipment tracking, and inspector management.

### Key Features

- User Management System with role-based access control
- Customer Relationship Management for tracking customers, contracts, and contacts
- Equipment Management with tracking and assignment capabilities
- Inspector Management including mobilization, drug testing, and class changes
- Administrative features for managing quick links, code types, and users

## Technology Stack

### Frontend
- Vue.js 3.x
- Quasar Framework 2.x
- Node.js 16+

### Backend
- ASP.NET Core 6.0
- SQL Server 2019+

### Cloud Infrastructure
- Microsoft Azure
  - Azure App Service
  - Azure SQL Database
  - Azure Storage
  - OneDrive Integration

## Project Structure

```
project-root/
├── src/
│   ├── web/          # Vue.js frontend application
│   ├── backend/      # ASP.NET Core backend services
│   └── shared/       # Shared types and utilities
├── infrastructure/   # Azure/Terraform configuration
└── tests/           # Unit, integration, and e2e tests
```

## Getting Started

### Prerequisites

1. Install Node.js (v16+)
2. Install .NET SDK 6.0
3. Install SQL Server 2019+
4. Azure CLI

### Environment Setup

1. Clone the repository
```bash
git clone <repository-url>
cd <project-directory>
```

2. Frontend Setup
```bash
cd src/web
npm install
```

3. Backend Setup
```bash
cd src/backend
dotnet restore
```

4. Configure Environment Variables
- Create `.env` files for development/production
- Set up Azure connection strings
- Configure authentication providers

### Local Development

1. Start the backend server:
```bash
cd src/backend
dotnet run
```

2. Start the frontend development server:
```bash
cd src/web
npm run dev
```

## Development Workflow

### Code Standards

- Follow Vue.js Style Guide
- Use C# coding conventions
- Implement comprehensive error handling
- Write unit tests for new features

### Branch Strategy

- main: Production-ready code
- develop: Integration branch
- feature/*: New features
- bugfix/*: Bug fixes
- release/*: Release candidates

### Commit Convention

```
type(scope): description

[optional body]

[optional footer]
```

Types: feat, fix, docs, style, refactor, test, chore

## Testing

### Frontend Testing
- Unit tests: Jest
- E2E tests: Cypress
- Component testing: Vue Test Utils

### Backend Testing
- Unit tests: xUnit
- Integration tests: TestServer
- API tests: Postman/Newman

### Coverage Requirements
- Minimum 80% code coverage
- Critical paths require 100% coverage

## Deployment

### Azure Deployment

1. Infrastructure Setup
```bash
cd infrastructure
terraform init
terraform apply
```

2. Database Migration
```bash
cd src/backend
dotnet ef database update
```

3. Application Deployment
- Configure CI/CD pipelines
- Set up environment variables
- Enable monitoring

### Environment Configuration

- Development: Local resources
- Staging: Azure staging slot
- Production: Azure production environment

## Infrastructure

### Azure Resources

- App Service Plan
- SQL Database
- Storage Account
- Application Insights
- Key Vault

### Security Configuration

- SSL/TLS encryption
- Role-based access control
- Azure AD integration
- SQL Always Encrypted

## Contributing

### Process

1. Fork the repository
2. Create feature branch
3. Implement changes
4. Write/update tests
5. Submit pull request

### Documentation

- Update README.md for new features
- Maintain API documentation
- Document configuration changes
- Update deployment guides

### Security Guidelines

- No secrets in code
- Use Azure Key Vault
- Implement least privilege
- Regular security audits

## Support

### Issue Reporting

- Use issue templates
- Provide reproduction steps
- Include environment details
- Attach relevant logs

### Feature Requests

- Use feature request template
- Provide business justification
- Include acceptance criteria
- Consider implementation impact

## License

Proprietary - All rights reserved

## Version Control

Documentation version: 1.0.0
Last updated: 2024