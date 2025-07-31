# Authorization Configuration

This folder contains a focused configuration approach that separates only the authorization policies from Program.cs for better maintainability as the policy list grows.

## Structure

### Authorization Extensions

#### `AuthorizationExtensions`
- **Purpose**: Manages all authorization policies and handlers separately from Program.cs
- **Features**:
  - Permission-based policies (user, employee, product, order management)
  - Role-based policies (Administrator, SystemAdmin, Manager, Employee, User)
  - Company access policies
  - Authorization handler registration
  - Clean extension method for easy integration

## Usage in Program.cs

Instead of having 30+ lines of authorization configuration in Program.cs:

```csharp
// Old approach - clutters Program.cs
builder.Services.AddAuthorization(options =>
{
    // 30+ lines of policies...
});
// 3 more lines of handler registration...
```

Now we have a clean single line:

```csharp
// New approach - clean and maintainable
builder.Services.AddAuthorizationPolicies();
```

## Benefits

1. **Clean Program.cs**: Removes 30+ lines of authorization configuration
2. **Maintainability**: Easy to add new policies without cluttering Program.cs
3. **Organization**: All authorization policies in one dedicated place
4. **Scalability**: Can easily grow to 100+ policies without impacting readability
5. **Focused Approach**: Only separates what makes sense to separate

## Adding New Policies

To add new authorization policies, simply modify the `AuthorizationExtensions.cs` file:

```csharp
// Add to the appropriate method (ConfigurePermissionPolicies, ConfigureRolePolicies, etc.)
options.AddPolicy("Permission.newfeature.read", policy =>
    policy.Requirements.Add(new PermissionRequirement("newfeature.read")));
```

## Policy Categories

### Permission Policies
- `Permission.factoryflowtracker.user.*` - User management
- `Permission.employee.*` - Employee management  
- `Permission.product.*` - Product management
- `Permission.order.*` - Order management

### Role Policies
- `Role.Administrator` - Full system access
- `Role.SystemAdmin` - System administration
- `Role.Manager` - Management functions
- `Role.Employee` - Employee-level access
- `Role.User` - Basic user access

### Company Access
- `CompanyAccess` - Multi-tenant company access control

This focused approach keeps Program.cs clean while providing a dedicated space for the authorization configuration that will inevitably grow over time.