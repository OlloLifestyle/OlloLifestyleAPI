using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OlloLifestyleAPI.Infrastructure.Data;
using OlloLifestyleAPI.Infrastructure.Persistence;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("OlloLifestyleAPI/appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Setup Master DbContext
        var masterOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        masterOptionsBuilder.UseSqlServer(connectionString);
        
        using var masterContext = new AppDbContext(masterOptionsBuilder.Options);
        
        Console.WriteLine("Clearing existing data...");
        
        // Clear existing data in proper order
        masterContext.UserCompanies.RemoveRange(masterContext.UserCompanies);
        masterContext.UserRoles.RemoveRange(masterContext.UserRoles);
        masterContext.RolePermissions.RemoveRange(masterContext.RolePermissions);
        masterContext.Users.RemoveRange(masterContext.Users);
        masterContext.Permissions.RemoveRange(masterContext.Permissions);
        masterContext.Roles.RemoveRange(masterContext.Roles);
        masterContext.Companies.RemoveRange(masterContext.Companies);
        
        await masterContext.SaveChangesAsync();
        
        Console.WriteLine("Seeding Master data...");
        await SeedData.SeedMasterDataAsync(masterContext);
        
        Console.WriteLine("Seeding Tenant data...");
        var tenantConnectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleAPI_Tenant;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";
        
        // Setup Tenant DbContext
        var tenantOptionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        tenantOptionsBuilder.UseSqlServer(tenantConnectionString);
        
        using var tenantContext = new CompanyDbContext(tenantOptionsBuilder.Options);
        
        // Clear existing tenant data
        tenantContext.OrderItems.RemoveRange(tenantContext.OrderItems);
        tenantContext.Orders.RemoveRange(tenantContext.Orders);
        tenantContext.Employees.RemoveRange(tenantContext.Employees);
        tenantContext.Products.RemoveRange(tenantContext.Products);
        
        await tenantContext.SaveChangesAsync();
        
        await SeedData.SeedTenantDataAsync(tenantConnectionString);
        
        Console.WriteLine("Database seeding completed successfully!");
    }
}