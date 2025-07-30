using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;
using OlloLifestyleAPI.Infrastructure.Services.Tenant;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancyServices(this IServiceCollection services)
    {
        // Add Memory Cache for tenant caching
        services.AddMemoryCache();
        
        // Add Tenant Service
        services.AddScoped<ITenantService, TenantService>();
        
        // Add Company DbContext Factory
        services.AddScoped<CompanyDbFactory>();

        return services;
    }
}