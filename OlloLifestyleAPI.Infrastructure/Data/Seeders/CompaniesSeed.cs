using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class CompaniesSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Companies.AnyAsync())
            return;

        var companies = new[]
        {
            new Company
            {
                Name = "Acme Corporation",
                DatabaseName = "OlloLifestyle_Acme",
                ConnectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_Acme;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ContactEmail = "admin@acme.com",
                ContactPhone = "+1-555-0100"
            },
            new Company
            {
                Name = "Global Tech Ltd",
                DatabaseName = "OlloLifestyle_GlobalTech",
                ConnectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_GlobalTech;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ContactEmail = "admin@globaltech.com",
                ContactPhone = "+1-555-0200"
            }
        };

        context.Companies.AddRange(companies);
        await context.SaveChangesAsync();
    }
}