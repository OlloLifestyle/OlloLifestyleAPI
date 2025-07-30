using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using OlloLifestyleAPI.Infrastructure.Extensions.Auth;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core infrastructure services (Database, etc.)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration);

        return services;
    }

    /// <summary>
    /// Adds authentication and user management services
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Add Auth services directly
        services.AddAuthServices();
        services.AddAuthRepositories();
        
        return services;
    }

    /// <summary>
    /// Adds business domain modules
    /// </summary>
    public static IServiceCollection AddDomainModules(this IServiceCollection services)
    {
        // TODO: Activate when domain implementations are ready
        // services.AddFactoryFlowTrackerModule();

        return services;
    }

    /// <summary>
    /// Adds application services (AutoMapper, Validation, Services)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(OlloLifestyleAPI.Application.Mappings.MasterMappingProfile), 
                              typeof(OlloLifestyleAPI.Application.Mappings.TenantMappingProfile));
        
        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(OlloLifestyleAPI.Application.Validators.Master.LoginRequestValidator).Assembly);
        
        return services;
    }
}