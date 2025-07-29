using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OlloLifestyleAPI.Infrastructure.Persistence.DesignTime;

public class CompanyDbContextFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        
        // Use the actual connection string for tenant database
        var connectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleAPI_Tenant;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new CompanyDbContext(optionsBuilder.Options);
    }
}