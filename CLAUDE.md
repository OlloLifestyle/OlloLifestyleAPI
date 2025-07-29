## Project Memories

- Remember layer project: A project involving layer management or layer-based architecture

## Project Overview

This is a production-grade .NET 9.0 ASP.NET Core Web API using Clean Architecture with multi-tenancy support:

- **OlloLifestyleAPI.Core** - Domain entities, interfaces, and DTOs
- **OlloLifestyleAPI.Application** - Business logic, services, validation, and mapping
- **OlloLifestyleAPI.Infrastructure** - Data access, external services, and cross-cutting concerns
- **OlloLifestyleAPI** - Web API layer with controllers, middleware, and configuration

### Key Components
- **Repository Pattern**: Generic repository with Unit of Work (Master/Tenant separation)
- **AutoMapper**: Entity to DTO mapping
- **FluentValidation**: Request validation
- **Serilog**: Structured logging with file and console output
- **Global Exception Handling**: Enhanced centralized error management with correlation IDs
- **Swagger Documentation**: API documentation with JWT auth support
- **JWT Authentication**: Role and permission-based authentication with comprehensive claims
- **Multi-Tenancy**: Company-based tenant isolation with separate repositories

## Architecture Improvements (Latest Update)

### 1. Domain Layer Enhancements
- **Base Entities**: Created `BaseEntity`, `BaseEntityGuid`, `AuditableEntity`, `AuditableEntityGuid`
- **Common Interfaces**: Added `IAuditable` and `ISoftDeletable` interfaces
- **Entity Updates**: Updated core entities to inherit from base classes
- **Clean Entities**: Removed Data Annotations since FluentValidation handles validation

### 2. Repository Pattern Reorganization
- **Proper Folder Separation**: All repositories now organized in `Master/` and `Tenant/` folders
- **Enhanced Generic Repository**: Added pagination, queryable access, and Guid key support
- **Multi-Context Support**: `MasterGenericRepository<T>` and `TenantGenericRepository<T>`
- **Unit of Work**: Separate `MasterUnitOfWork` and `TenantUnitOfWork` implementations
- **Service Registration**: Updated DI container to use proper repository structure

### 3. Data Seeders Organization
- **Domain-Based Seeders**: Organized into separate classes by domain area
  - `CompaniesSeed`, `RolesSeed`, `PermissionsSeed`, `UserSeed`
  - `RolePermissionsSeed`, `UserRolesSeed`, `UserCompaniesSeed`
  - Tenant seeders: `ProductsSeed`, `EmployeesSeed`, `OrdersSeed`
- **Dependency Management**: Proper seeding order to handle foreign key relationships

### 4. Enhanced JWT Authentication
- **Role-Based Claims**: JWT tokens include user roles and permissions
- **Permission Claims**: Granular permission claims for fine-grained access control
- **Company Access**: Company-specific claims for multi-tenant access
- **Custom Claims**: Added `user_type`, `is_admin`, and module-specific permissions

### 5. Authorization Enhancements
- **Custom Attributes**: `RequirePermissionAttribute`, `RequireRoleAttribute`, `RequireCompanyAccessAttribute`
- **Claim-Based Authorization**: Comprehensive claim validation for controllers

### 6. Middleware Improvements
- **Enhanced Exception Handling**: Added correlation IDs, trace IDs, and environment-specific error details
- **Tenant Resolution**: Updated to work with new JWT claim structure
- **Audit Interceptor**: Automatic audit field population for entities

### 7. Infrastructure Enhancements
- **Audit Interceptor**: Automatically handles CreatedAt, UpdatedAt, CreatedBy, UpdatedBy fields
- **Soft Delete Support**: Built-in soft delete functionality through ISoftDeletable
- **Connection Management**: Improved database context handling for multi-tenancy

## Usage Guidelines

### Repository Usage
```csharp
// For Master context (Users, Companies, Roles)
public class MyService
{
    private readonly IMasterUnitOfWork _unitOfWork;
    
    public async Task<User> GetUserAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }
}

// For Tenant context (Employees, Products, Orders)
public class TenantService
{
    private readonly ITenantUnitOfWork _unitOfWork;
    
    public async Task<Employee> GetEmployeeAsync(Guid id)
    {
        return await _unitOfWork.Employees.GetByIdAsync(id);
    }
}
```

### FluentValidation Usage
```csharp
// Example validator for Employee
public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
    }
}
```

### Authorization Usage
```csharp
[RequirePermission("employee.read")]
public async Task<IActionResult> GetEmployees()
{
    // Only users with employee.read permission can access
}

[RequireRole("Administrator", "Manager")]
public async Task<IActionResult> ManageUsers()
{
    // Only Administrators or Managers can access
}
```

## Best Practices Implemented

1. **Separation of Concerns**: Clear separation between Master and Tenant contexts with proper folder structure
2. **Domain-Driven Design**: Clean entities without validation attributes, using FluentValidation for validation
3. **Security**: Comprehensive role and permission-based authorization
4. **Auditability**: Automatic audit trail for all entities via interceptors
5. **Error Handling**: Structured error responses with correlation tracking
6. **Multi-Tenancy**: Company-based isolation with proper context switching
7. **Validation**: FluentValidation for clean separation between domain and validation logic

## Repository Structure
- `Repositories/Master/`: Master database repositories (Users, Companies, Roles)
- `Repositories/Tenant/`: Tenant database repositories (Employees, Products, Orders)
- All repositories properly organized by context