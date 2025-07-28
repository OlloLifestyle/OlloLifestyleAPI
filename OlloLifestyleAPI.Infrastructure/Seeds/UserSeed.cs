using Microsoft.AspNetCore.Identity;
using OlloLifestyleAPI.Core.Entities;

namespace OlloLifestyleAPI.Infrastructure.Seeds;

public static class UserSeed
{
    public static async Task SeedAsync(UserManager<User> userManager)
    {
        if (!userManager.Users.Any())
        {
            var superAdmin = new User
            {
                FirstName = "Super",
                LastName = "Admin",
                UserName = "admin@ollolifestyle.com",
                Email = "admin@ollolifestyle.com",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(superAdmin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }

            var testUser = new User
            {
                FirstName = "Test",
                LastName = "User",
                UserName = "user@test.com",
                Email = "user@test.com",
                EmailConfirmed = true,
                PhoneNumber = "+1234567891",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var userResult = await userManager.CreateAsync(testUser, "User@123");
            if (userResult.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }
    }
}