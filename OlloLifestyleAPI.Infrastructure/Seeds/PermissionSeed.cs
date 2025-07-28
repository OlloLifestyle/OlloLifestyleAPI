using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities;
using OlloLifestyleAPI.Infrastructure.DbContexts;

namespace OlloLifestyleAPI.Infrastructure.Seeds;

public static class PermissionSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new[]
            {
                // User Management
                new Permission { Name = "users.view", Description = "View users", Module = "UserManagement" },
                new Permission { Name = "users.create", Description = "Create users", Module = "UserManagement" },
                new Permission { Name = "users.edit", Description = "Edit users", Module = "UserManagement" },
                new Permission { Name = "users.delete", Description = "Delete users", Module = "UserManagement" },

                // Role Management
                new Permission { Name = "roles.view", Description = "View roles", Module = "RoleManagement" },
                new Permission { Name = "roles.create", Description = "Create roles", Module = "RoleManagement" },
                new Permission { Name = "roles.edit", Description = "Edit roles", Module = "RoleManagement" },
                new Permission { Name = "roles.delete", Description = "Delete roles", Module = "RoleManagement" },

                // Product Management
                new Permission { Name = "products.view", Description = "View products", Module = "ProductManagement" },
                new Permission { Name = "products.create", Description = "Create products", Module = "ProductManagement" },
                new Permission { Name = "products.edit", Description = "Edit products", Module = "ProductManagement" },
                new Permission { Name = "products.delete", Description = "Delete products", Module = "ProductManagement" },

                // Company Management
                new Permission { Name = "companies.view", Description = "View companies", Module = "CompanyManagement" },
                new Permission { Name = "companies.create", Description = "Create companies", Module = "CompanyManagement" },
                new Permission { Name = "companies.edit", Description = "Edit companies", Module = "CompanyManagement" },
                new Permission { Name = "companies.delete", Description = "Delete companies", Module = "CompanyManagement" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}