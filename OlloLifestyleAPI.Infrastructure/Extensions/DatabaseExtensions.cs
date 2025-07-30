using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Infrastructure.Interceptors;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add interceptors
        services.AddScoped<AuditInterceptor>();

        // Add AppDbContext (Shared Identity Database)
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

        // Add CompanyDbContext (Tenant-specific Database)
        services.AddDbContext<CompanyDbContext>((serviceProvider, options) =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"), // Use same connection for demo
                b => b.MigrationsAssembly(typeof(CompanyDbContext).Assembly.FullName))
            .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

        // Add CompanyDbFactory for tenant context creation
        services.AddScoped<CompanyDbFactory>();

        return services;
    }
}