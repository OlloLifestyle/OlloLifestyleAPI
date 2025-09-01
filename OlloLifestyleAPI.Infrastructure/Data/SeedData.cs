using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OlloLifestyleAPI.Infrastructure.Data.Seeders;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedMasterDataAsync(AppDbContext context)
    {
        // Only ensure database creation and seed data in Development environment
        // In Production, database should already exist with proper structure
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environment == "Development")
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed data in order due to dependencies
            await CompaniesSeed.SeedAsync(context);
            await RolesSeed.SeedAsync(context); 
            await PermissionsSeed.SeedAsync(context);
            await UserSeed.SeedAsync(context);
            await RolePermissionsSeed.SeedAsync(context);
            await UserRolesSeed.SeedAsync(context);
            await UserCompaniesSeed.SeedAsync(context);
        }
    }

    public static async Task SeedTenantDataAsync(string connectionString)
    {
        // Only seed tenant data in Development environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environment == "Development")
        {
            var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new CompanyDbContext(optionsBuilder.Options);
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // TODO: Add User seed data when needed
        }
    }
}