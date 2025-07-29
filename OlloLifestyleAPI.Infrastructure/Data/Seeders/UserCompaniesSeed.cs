using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class UserCompaniesSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.UserCompanies.AnyAsync())
            return;

        var users = await context.Users.ToListAsync();
        var companies = await context.Companies.ToListAsync();

        var userCompanies = new[]
        {
            new UserCompany 
            { 
                UserId = users[0].Id, 
                CompanyId = companies[0].Id, 
                AssignedAt = DateTime.UtcNow 
            },
            new UserCompany 
            { 
                UserId = users[1].Id, 
                CompanyId = companies[1].Id, 
                AssignedAt = DateTime.UtcNow 
            }
        };

        context.UserCompanies.AddRange(userCompanies);
        await context.SaveChangesAsync();
    }
}