using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Services.Master;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class UserSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new[]
        {
            new User
            {
                UserName = "admin@acme.com",
                PasswordHash = AuthService.HashPassword("Admin123!"),
                FirstName = "John",
                LastName = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserName = "admin@globaltech.com",
                PasswordHash = AuthService.HashPassword("Admin123!"),
                FirstName = "Mike",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}