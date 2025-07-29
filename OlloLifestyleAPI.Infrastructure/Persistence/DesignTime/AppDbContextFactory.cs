using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OlloLifestyleAPI.Infrastructure.Persistence.DesignTime;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use the actual connection string from appsettings.json
        var connectionString = "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleAPI_Master;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";
        
        optionsBuilder.UseSqlServer(connectionString, 
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}