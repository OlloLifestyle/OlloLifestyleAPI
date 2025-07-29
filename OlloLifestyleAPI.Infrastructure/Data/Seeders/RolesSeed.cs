using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class RolesSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new[]
        {
            new Role
            {
                Name = "Administrator",
                Description = "System administrator with full access",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Manager",
                Description = "Department manager with limited admin access",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Employee",
                Description = "Standard employee access",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
    }
}