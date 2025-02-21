# Backend Application Documentation

## Overview

This ASP.NET Core backend application provides a robust API for managing users, customers, equipment, and inspectors. Built with enterprise-grade security, scalability, and maintainability in mind, it implements a clean architecture pattern to ensure separation of concerns and maintainable code.

## Project Structure

The solution follows clean architecture principles with the following structure:

```
Backend.sln
├── src/
│   ├── Core/                 # Domain models, interfaces, business logic
│   │   ├── Entities/        # Domain entities
│   │   ├── Interfaces/      # Repository and service interfaces
│   │   └── Services/        # Business logic implementations
│   ├── Infrastructure/      # External concerns implementation
│   │   ├── Data/           # EF Core configurations
│   │   ├── Services/       # External service implementations
│   │   └── Identity/       # Authentication/Authorization
│   └── API/                # API Controllers and middleware
├── tests/
│   ├── Unit/
│   ├── Integration/
│   └── Performance/
```

## Prerequisites

- .NET 6.0 SDK or later
- SQL Server 2019+
- Docker Desktop
- Azure CLI
- Visual Studio 2022 or JetBrains Rider
- Git

## Getting Started

1. Clone the repository
2. Configure user secrets:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;..."
dotnet user-secrets set "AzureAd:ClientId" "your-client-id"
dotnet user-secrets set "AzureAd:ClientSecret" "your-client-secret"
```

3. Run database migrations:
```bash
dotnet ef database update
```

4. Start the application:
```bash
dotnet run --project src/API/API.csproj
```

## Development Workflow

### Coding Standards
- Follow Microsoft's C# coding conventions
- Use async/await for all I/O operations
- Implement comprehensive error handling
- Document public APIs using XML comments

### Git Workflow
1. Create feature branch from develop
2. Implement changes with atomic commits
3. Submit PR with required reviewers
4. Ensure CI pipeline passes
5. Merge after approval

### Pull Request Requirements
- Unit test coverage > 80%
- No security vulnerabilities
- Passes static code analysis
- Updated documentation
- Changelog entry

## API Documentation

### Authentication
- JWT Bearer token authentication
- Azure AD integration
- Role-based authorization

### Error Handling
```json
{
    "error": {
        "code": "ERROR_CODE",
        "message": "User-friendly message",
        "details": "Technical details"
    }
}
```

### Rate Limiting
- 1000 requests/minute for authenticated users
- 100 requests/minute for anonymous users

## Database

### Entity Relationships
- Users -> Roles (Many-to-Many)
- Customers -> Contacts (One-to-Many)
- Customers -> Contracts (One-to-Many)
- Equipment -> Inspectors (Many-to-Many)

### Optimization Strategies
- Indexed views for frequently accessed data
- Materialized paths for hierarchical data
- Query optimization using INCLUDES
- Appropriate indexing strategy

## Security

### Permission System
- Role-based access control (RBAC)
- Fine-grained permissions
- Resource-based authorization

### Data Protection
- TLS 1.3 for transport security
- AES-256 for sensitive data encryption
- SQL parameters to prevent injection
- Input validation and sanitization

## Testing

### Unit Tests
- xUnit test framework
- Moq for mocking
- Fluent Assertions
- Arrange-Act-Assert pattern

### Integration Tests
- TestServer for in-memory testing
- Separate test database
- Realistic test data
- Transaction rollback after tests

## Monitoring

### Logging
- Serilog implementation
- Structured logging format
- Log levels: Debug, Info, Warning, Error
- Azure Application Insights integration

### Metrics
- Request duration
- Error rates
- Database connection pool
- Memory usage
- CPU utilization

## Deployment

### Environment Configuration
- Development
- Staging
- Production

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "API.dll"]
```

## Third-Party Services

### Azure Storage Integration
- Blob storage for file management
- Queue storage for background jobs
- Table storage for diagnostics

### OneDrive Integration
- Microsoft Graph API integration
- Automatic folder creation
- File management operations

### Email Service
- SMTP configuration
- Email templates
- Attachment handling
- Queue-based processing

## Troubleshooting

### Common Issues
1. Database Connection
   - Check connection string
   - Verify network connectivity
   - Ensure SQL Server is running

2. Authentication
   - Validate token configuration
   - Check Azure AD settings
   - Verify role assignments

### Debugging
- Use Application Insights
- Check system logs
- Enable detailed errors in development

## Disaster Recovery

### Backup Procedures
- Daily full backups
- Hourly differential backups
- Transaction log shipping
- Point-in-time recovery support

### Recovery Steps
1. Restore latest backup
2. Apply transaction logs
3. Verify data integrity
4. Update DNS if needed
5. Monitor system health

## Support

For technical support or questions, contact:
- Email: backend-support@company.com
- Teams: Backend Support Channel