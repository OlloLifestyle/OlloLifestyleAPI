using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Infrastructure.Interceptors;
using OlloLifestyleAPI.Infrastructure.Persistence;

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

        return services;
    }
}