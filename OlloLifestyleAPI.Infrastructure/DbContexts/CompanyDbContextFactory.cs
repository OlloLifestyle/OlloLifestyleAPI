using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OlloLifestyleAPI.Core.Interfaces;

namespace OlloLifestyleAPI.Infrastructure.DbContexts;

public class CompanyDbContextFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        
        // Use a default connection string for design-time operations
        optionsBuilder.UseSqlServer("Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleCompany;Trusted_Connection=True;TrustServerCertificate=True;");

        // Create a mock tenant provider for design-time
        var mockTenantProvider = new MockTenantProvider();

        return new CompanyDbContext(optionsBuilder.Options, mockTenantProvider);
    }
}

// Mock tenant provider for design-time operations
public class MockTenantProvider : ITenantProvider
{
    public string? GetCurrentTenantConnectionString()
    {
        return "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleCompany;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    public int? GetCurrentTenantId()
    {
        return 1;
    }

    public void SetTenant(int tenantId, string connectionString)
    {
        // No-op for design-time
    }
}