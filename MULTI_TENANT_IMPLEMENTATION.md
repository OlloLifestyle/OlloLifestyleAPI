# Multi-Tenant Architecture Implementation Guide

## Overview
This implementation provides a complete multi-tenant architecture for the OlloLifestyle API with the following key features:

- **Shared Identity Database (AppDbContext)**: Manages users, companies, roles, and permissions
- **Per-Tenant Business Databases (CompanyDbContext)**: Separate databases for each company's business data
- **JWT-based Authentication**: With CompanyId claims for tenant resolution
- **Middleware-based Tenant Resolution**: Automatic tenant context setting
- **Repository Pattern**: Clean separation of concerns with tenant-aware data access

## Architecture Components

### 1. Database Structure

#### AppDbContext (Master/Shared Database)
- **Companies**: Tenant configuration with connection strings
- **Users**: Authentication users linked to companies
- **Roles**: System and custom roles
- **Permissions**: Fine-grained access control
- **UserRoles**: User role assignments
- **RolePermissions**: Role permission assignments

#### CompanyDbContext (Per-Tenant Database)
- **Employees**: Company-specific employee data
- **Orders**: Business orders
- **Products**: Product catalog
- **OrderItems**: Order line items

### 2. Authentication Flow

1. User logs in with email/password via `/api/auth/login`
2. System validates credentials against AppDbContext
3. JWT token generated with CompanyId claim
4. Subsequent requests include JWT token
5. TenantMiddleware extracts CompanyId and sets tenant context
6. Business operations use tenant-specific database

### 3. Key Services

- **ITenantService**: Manages tenant context and caching
- **IAuthService**: Handles authentication and JWT generation
- **IEmployeeService**: Business logic for employee operations
- **ICompanyRepository**: Data access for tenant-specific operations

## Testing & Validation

### Sample Test Data

The system includes seed data for testing:

#### Companies:
1. **Acme Corporation**
   - Domain: acme.com
   - Database: OlloLifestyle_Acme
   - Admin: admin@acme.com / Admin123!
   - Manager: manager@acme.com / Manager123!

2. **Global Tech Ltd**
   - Domain: globaltech.com
   - Database: OlloLifestyle_GlobalTech
   - Admin: admin@globaltech.com / Admin123!

### Testing Steps

#### 1. Authentication Test
```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@acme.com",
  "password": "Admin123!"
}
```

Expected Response:
```json
{
  "token": "eyJ...",
  "expiresAt": "2024-01-01T00:00:00Z",
  "user": {
    "id": "guid",
    "email": "admin@acme.com",
    "firstName": "John",
    "lastName": "Admin",
    "isActive": true
  },
  "company": {
    "id": "guid",
    "name": "Acme Corporation",
    "domain": "acme.com",
    "isActive": true
  }
}
```

#### 2. Tenant Context Test
```bash
GET /api/employees/tenant-info
Authorization: Bearer {token}
```

Expected Response:
```json
{
  "companyId": "guid",
  "companyName": "Acme Corporation",
  "databaseName": "OlloLifestyle_Acme",
  "domain": "acme.com",
  "isActive": true
}
```

#### 3. Multi-Tenant Data Isolation Test
```bash
# Login as Acme admin
POST /api/auth/login
{
  "email": "admin@acme.com",
  "password": "Admin123!"
}

# Get Acme employees (should return Acme data only)
GET /api/employees
Authorization: Bearer {acme_token}

# Login as GlobalTech admin
POST /api/auth/login
{
  "email": "admin@globaltech.com",
  "password": "Admin123!"
}

# Get GlobalTech employees (should return GlobalTech data only)
GET /api/employees
Authorization: Bearer {globaltech_token}
```

#### 4. Employee CRUD Operations Test
```bash
# Create Employee
POST /api/employees
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": "new.employee@acme.com",
  "firstName": "New",
  "lastName": "Employee",
  "employeeNumber": "EMP003",
  "department": "Sales",
  "position": "Sales Rep",
  "salary": 50000,
  "phone": "+1-555-0103"
}

# Get All Employees
GET /api/employees
Authorization: Bearer {token}

# Get Employee by ID
GET /api/employees/{id}
Authorization: Bearer {token}

# Update Employee
PUT /api/employees/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "salary": 55000,
  "position": "Senior Sales Rep"
}

# Get Employees by Department
GET /api/employees/department/Sales
Authorization: Bearer {token}

# Delete Employee (Soft Delete)
DELETE /api/employees/{id}
Authorization: Bearer {token}
```

### Validation Checklist

#### ✅ Security Validation
- [ ] Users can only access data from their own company
- [ ] JWT tokens contain CompanyId claims
- [ ] Inactive companies cannot be accessed
- [ ] Inactive users cannot login
- [ ] Password hashing is secure (BCrypt)

#### ✅ Multi-Tenancy Validation
- [ ] Each company uses separate database
- [ ] Tenant context is properly resolved from JWT
- [ ] Data isolation between tenants is enforced
- [ ] Caching works correctly per tenant
- [ ] Connection strings are tenant-specific

#### ✅ API Functionality Validation
- [ ] Authentication endpoints work correctly
- [ ] Employee CRUD operations work
- [ ] Error handling is appropriate
- [ ] Authorization middleware functions
- [ ] Tenant middleware resolves context

#### ✅ Database Validation
- [ ] Master database migrations apply correctly
- [ ] Tenant databases are created automatically
- [ ] Seed data populates correctly
- [ ] Foreign key constraints work
- [ ] Indexes are created properly

## Running the Application

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Apply migrations:**
   ```bash
   dotnet ef database update --project OlloLifestyleAPI.Infrastructure --startup-project OlloLifestyleAPI --context AppDbContext
   ```

3. **Run the application:**
   ```bash
   dotnet run --project OlloLifestyleAPI
   ```

4. **Access Swagger UI:**
   - Navigate to `https://localhost:7xxx/swagger`
   - Use the seed data credentials to test authentication

## Production Considerations

### Security
- Change JWT secret key to a strong, unique value
- Use proper password hashing (BCrypt with sufficient rounds)
- Implement rate limiting for authentication endpoints
- Use HTTPS in production
- Validate and sanitize all inputs

### Performance
- Implement connection pooling for tenant databases
- Add database indexes for frequently queried fields
- Consider read replicas for tenant databases
- Implement proper caching strategies
- Monitor database performance per tenant

### Scalability
- Consider database sharding for very large tenants
- Implement tenant database backup strategies
- Plan for tenant onboarding automation
- Consider microservices for different business domains
- Implement health checks for tenant databases

### Monitoring
- Log tenant-specific operations
- Monitor resource usage per tenant
- Set up alerts for tenant database issues
- Track authentication and authorization events
- Implement audit trails for sensitive operations

## Troubleshooting

### Common Issues

1. **"No tenant context found" Error**
   - Ensure JWT token contains CompanyId claim
   - Verify TenantMiddleware is registered after UseAuthentication
   - Check that user's company is active

2. **Database Connection Issues**
   - Verify connection strings in seed data
   - Ensure LocalDB is running
   - Check that tenant databases are created

3. **Authentication Failures**
   - Verify JWT configuration in appsettings.json
   - Check password hashing implementation
   - Ensure user accounts are active

4. **Data Isolation Problems**
   - Verify tenant middleware is working
   - Check repository implementations
   - Ensure CompanyDbFactory uses correct connection string

## Next Steps

1. **Add More Business Entities**: Extend tenant databases with additional business objects
2. **Implement Role-Based Authorization**: Use the permission system for fine-grained access control
3. **Add Tenant Management APIs**: Allow creating, updating, and managing tenant configurations
4. **Implement Audit Logging**: Track all tenant operations for compliance
5. **Add Data Export/Import**: Allow tenants to export/import their data
6. **Implement Tenant Billing**: Track usage and implement billing per tenant
7. **Add Tenant Customization**: Allow tenants to customize UI, workflows, etc.
8. **Implement Backup/Restore**: Automated backup and restore for tenant data