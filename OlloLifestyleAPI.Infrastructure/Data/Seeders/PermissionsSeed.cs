using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class PermissionsSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        var permissions = new[]
        {
            new Permission { Name = "employee.read", Description = "View employees", Module = "Employee", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "employee.write", Description = "Create/Update employees", Module = "Employee", Action = "Write", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "employee.delete", Description = "Delete employees", Module = "Employee", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "order.read", Description = "View orders", Module = "Order", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "order.write", Description = "Create/Update orders", Module = "Order", Action = "Write", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "product.read", Description = "View products", Module = "Product", Action = "Read", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "product.write", Description = "Create/Update products", Module = "Product", Action = "Write", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Permission { Name = "product.delete", Description = "Delete products", Module = "Product", Action = "Delete", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();
    }
}