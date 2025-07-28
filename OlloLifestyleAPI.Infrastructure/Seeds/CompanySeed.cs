using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities;
using OlloLifestyleAPI.Infrastructure.DbContexts;

namespace OlloLifestyleAPI.Infrastructure.Seeds;

public static class CompanySeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Companies.AnyAsync())
        {
            var companies = new[]
            {
                new Company
                {
                    Name = "Ollo Lifestyle Demo Company",
                    Code = "DEMO",
                    Description = "Demo company for testing purposes",
                    ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=OlloLifestyle_Demo;Trusted_Connection=true;MultipleActiveResultSets=true;",
                    IsActive = true,
                    CreatedBy = "system"
                },
                new Company
                {
                    Name = "Ollo Lifestyle Test Company",
                    Code = "TEST",
                    Description = "Test company for development",
                    ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=OlloLifestyle_Test;Trusted_Connection=true;MultipleActiveResultSets=true;",
                    IsActive = true,
                    CreatedBy = "system"
                }
            };

            await context.Companies.AddRangeAsync(companies);
            await context.SaveChangesAsync();
        }
    }
}