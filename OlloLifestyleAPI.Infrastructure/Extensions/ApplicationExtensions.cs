using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Infrastructure.Extensions.Application;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddApplicationMappings();
        services.AddApplicationValidation();
        services.AddApplicationDomainServices();

        return services;
    }
}