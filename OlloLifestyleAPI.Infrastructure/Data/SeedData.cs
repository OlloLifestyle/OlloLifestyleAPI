using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Services.Master;

namespace OlloLifestyleAPI.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedMasterDataAsync(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Companies.AnyAsync())
        {
            return; // Data already seeded
        }

        // Create sample companies
        var company1 = new Company
        {
            Name = "Acme Corporation",
            DatabaseName = "OlloLifestyle_Acme",
            ConnectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_Acme;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ContactEmail = "admin@acme.com",
            ContactPhone = "+1-555-0100"
        };

        var company2 = new Company
        {
            Name = "Global Tech Ltd",
            DatabaseName = "OlloLifestyle_GlobalTech",
            ConnectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_GlobalTech;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ContactEmail = "admin@globaltech.com",
            ContactPhone = "+1-555-0200"
        };

        context.Companies.AddRange(company1, company2);

        // Create sample roles
        var adminRole = new Role
        {
            Name = "Administrator",
            Description = "System administrator with full access",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var managerRole = new Role
        {
            Name = "Manager",
            Description = "Department manager with limited admin access",
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        var employeeRole = new Role
        {
            Name = "Employee",
            Description = "Standard employee access",
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(adminRole, managerRole, employeeRole);

        // Create sample permissions
        var permissions = new[]
        {
            new Permission { Name = "employee.read", Description = "View employees", Module = "Employee", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "employee.write", Description = "Create/Update employees", Module = "Employee", Action = "Write", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "employee.delete", Description = "Delete employees", Module = "Employee", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "order.read", Description = "View orders", Module = "Order", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "order.write", Description = "Create/Update orders", Module = "Order", Action = "Write", IsActive = true, CreatedAt = DateTime.UtcNow },
        };

        context.Permissions.AddRange(permissions);

        // Create sample users
        var user1 = new User
        {
            UserName = "admin@acme.com",
            PasswordHash = AuthService.HashPassword("Admin123!"),
            FirstName = "John",
            LastName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            UserName = "admin@globaltech.com",
            PasswordHash = AuthService.HashPassword("Admin123!"),
            FirstName = "Mike",
            LastName = "Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(user1, user2);

        // Save changes to get IDs
        await context.SaveChangesAsync();

        // Create user-company relationships
        var userCompanies = new[]
        {
            new UserCompany { UserId = user1.Id, CompanyId = company1.Id, AssignedAt = DateTime.UtcNow },
            new UserCompany { UserId = user2.Id, CompanyId = company2.Id, AssignedAt = DateTime.UtcNow }
        };

        context.UserCompanies.AddRange(userCompanies);

        // Create role assignments
        var userRoles = new[]
        {
            new UserRole { UserId = user1.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = user2.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow }
        };

        context.UserRoles.AddRange(userRoles);

        // Create role permissions (Admin gets all permissions)
        var rolePermissions = new List<RolePermission>();
        foreach (var permission in permissions)
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = adminRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        // Manager gets read/write permissions
        foreach (var permission in permissions.Where(p => p.Action != "Delete"))
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = managerRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        // Employee gets read permissions only
        foreach (var permission in permissions.Where(p => p.Action == "Read"))
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = employeeRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        context.RolePermissions.AddRange(rolePermissions);

        await context.SaveChangesAsync();
    }

    public static async Task SeedTenantDataAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new CompanyDbContext(optionsBuilder.Options);
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Employees.AnyAsync())
        {
            return; // Data already seeded
        }

        // Create sample products
        var products = new[]
        {
            new Core.Entities.Tenant.Product 
            { 
                Id = Guid.NewGuid(), 
                Name = "Laptop", 
                Description = "High-performance business laptop",
                Sku = "LAP001",
                Price = 1299.99m,
                StockQuantity = 50,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Core.Entities.Tenant.Product 
            { 
                Id = Guid.NewGuid(), 
                Name = "Office Chair", 
                Description = "Ergonomic office chair",
                Sku = "CHR001",
                Price = 299.99m,
                StockQuantity = 25,
                Category = "Furniture",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);

        // Create sample employees
        var employees = new[]
        {
            new Core.Entities.Tenant.Employee
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@company.com",
                FirstName = "John",
                LastName = "Doe",
                EmployeeNumber = "EMP001",
                Department = "IT",
                Position = "Software Developer",
                Salary = 75000m,
                HireDate = DateTime.UtcNow.AddYears(-2),
                IsActive = true,
                Phone = "+1-555-0101",
                Address = "123 Main St, City, State",
                CreatedAt = DateTime.UtcNow
            },
            new Core.Entities.Tenant.Employee
            {
                Id = Guid.NewGuid(),
                Email = "jane.smith@company.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmployeeNumber = "EMP002",
                Department = "HR",
                Position = "HR Manager",
                Salary = 65000m,
                HireDate = DateTime.UtcNow.AddYears(-3),
                IsActive = true,
                Phone = "+1-555-0102",
                Address = "456 Oak Ave, City, State",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // Create sample order
        var order = new Core.Entities.Tenant.Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD001",
            EmployeeId = employees[0].Id,
            CustomerName = "ABC Company",
            CustomerEmail = "orders@abccompany.com",
            TotalAmount = 1599.98m,
            OrderDate = DateTime.UtcNow.AddDays(-5),
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Create order items
        var orderItems = new[]
        {
            new Core.Entities.Tenant.OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = products[0].Id,
                Quantity = 1,
                UnitPrice = products[0].Price,
                TotalPrice = products[0].Price,
                CreatedAt = DateTime.UtcNow
            },
            new Core.Entities.Tenant.OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = products[1].Id,
                Quantity = 1,
                UnitPrice = products[1].Price,
                TotalPrice = products[1].Price,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();
    }
}