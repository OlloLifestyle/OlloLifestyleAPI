using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Infrastructure.Data.Seeders;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedMasterDataAsync(AppDbContext context)
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

    public static async Task SeedTenantDataAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new CompanyDbContext(optionsBuilder.Options);
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // TODO: Add User seed data when needed
    }
}