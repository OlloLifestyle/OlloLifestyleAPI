using Microsoft.AspNetCore.Identity;
using OlloLifestyleAPI.Core.Entities;

namespace OlloLifestyleAPI.Infrastructure.Seeds;

public static class RoleSeed
{
    public static async Task SeedAsync(RoleManager<Role> roleManager)
    {
        var roles = new[]
        {
            new Role { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Description = "Super Administrator with full system access" },
            new Role { Name = "Admin", NormalizedName = "ADMIN", Description = "Administrator with full company access" },
            new Role { Name = "Manager", NormalizedName = "MANAGER", Description = "Manager with limited administrative access" },
            new Role { Name = "User", NormalizedName = "USER", Description = "Standard user with basic access" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role);
            }
        }
    }
}