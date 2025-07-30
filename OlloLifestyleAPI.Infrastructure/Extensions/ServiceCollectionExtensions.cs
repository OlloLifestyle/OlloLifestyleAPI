using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration);
        services.AddMultiTenancyServices();
        services.AddRepositoryServices();
        services.AddAuthenticationServices();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddApplicationLayerServices();
        
        return services;
    }
}