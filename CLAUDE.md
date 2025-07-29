# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a production-grade .NET 9.0 ASP.NET Core Web API using Clean Architecture with multi-tenancy support:

- **OlloLifestyleAPI.Core** - Domain entities, interfaces, and DTOs
- **OlloLifestyleAPI.Application** - Business logic, services, validation, and mapping
- **OlloLifestyleAPI.Infrastructure** - Data access, external services, and cross-cutting concerns
- **OlloLifestyleAPI** - Web API layer with controllers, middleware, and configuration

## Development Commands

### Database Operations
```bash
# Add migration for Identity database
dotnet ef migrations add InitialCreate --context AppDbContext --project OlloLifestyleAPI.Infrastructure --startup-project OlloLifestyleAPI

# Update Identity database
dotnet ef database update --context AppDbContext --project OlloLifestyleAPI.Infrastructure --startup-project OlloLifestyleAPI

# Add migration for Company database (tenant-specific)
dotnet ef migrations add InitialCompanyCreate --context CompanyDbContext --project OlloLifestyleAPI.Infrastructure --startup-project OlloLifestyleAPI

# Update Company database
dotnet ef database update --context CompanyDbContext --project OlloLifestyleAPI.Infrastructure --startup-project OlloLifestyleAPI
```

### Build and Run
```bash
# Build the entire solution
dotnet build OlloLifestyleAPI.sln

# Run the API (from root directory)
dotnet run --project OlloLifestyleAPI/OlloLifestyleAPI.csproj

# Run with specific environment
dotnet run --project OlloLifestyleAPI/OlloLifestyleAPI.csproj --environment Development
```

### Testing API
- Swagger UI available at root URL when running in development
- Default admin user: admin@ollolifestyle.com / Admin@123
- Default test user: user@test.com / User@123

## Architecture Features

### Multi-Tenancy
- **Identity Database**: Contains users, roles, permissions, and company information
- **Company Databases**: Tenant-specific business data
- **Dynamic Context Switching**: Middleware resolves tenant based on authenticated user
- **Tenant Provider**: Manages current tenant context throughout request lifecycle

### Authentication & Authorization
- **JWT Bearer Authentication**: With refresh token support
- **Role-Based Authorization**: SuperAdmin, Admin, Manager, User roles
- **Permission-Based Authorization**: Granular permissions per module
- **Multi-Factor Support**: Ready for extension with additional auth methods

### Key Components
- **Repository Pattern**: Generic repository with Unit of Work
- **AutoMapper**: Entity to DTO mapping
- **FluentValidation**: Request validation
- **Serilog**: Structured logging with file and console output
- **Global Exception Handling**: Centralized error management
- **Swagger Documentation**: API documentation with JWT auth support

### Database Contexts
- **AppDbContext**: Identity management, roles, permissions, companies
- **CompanyDbContext**: Tenant-specific business data with audit trails

### Middleware Pipeline
1. Global Exception Middleware
2. Authentication
3. Tenant Resolution Middleware
4. Authorization

### Security Features
- Password complexity requirements
- Account lockout policies
- JWT token expiration and refresh
- Permission-based access control
- Secure password hashing with Identity

### Configuration
- **Connection Strings**: Separate for Identity and tenant databases
- **JWT Settings**: Configurable tokens with proper security
- **Logging**: Structured logging with Serilog
- **Environment-specific**: Development and production configurations

### API Endpoints
- **Auth**: Login, register, refresh token, logout, change password
- **Products**: CRUD operations with tenant isolation
- **Permissions**: View permissions by module (admin only)

When extending the API:
1. Add entities to appropriate context (Identity vs Company)
2. Create DTOs in Core layer
3. Implement validators in Application layer
4. Add AutoMapper profiles
5. Create services following existing patterns
6. Add controllers with proper authorization
7. Update permissions and policies as needed