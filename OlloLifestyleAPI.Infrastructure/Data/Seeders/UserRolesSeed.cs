using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class UserRolesSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.UserRoles.AnyAsync())
            return;

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");
        var users = await context.Users.ToListAsync();

        var userRoles = new List<UserRole>();
        
        foreach (var user in users)
        {
            userRoles.Add(new UserRole 
            { 
                UserId = user.Id, 
                RoleId = adminRole.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        context.UserRoles.AddRange(userRoles);
        await context.SaveChangesAsync();
    }
}