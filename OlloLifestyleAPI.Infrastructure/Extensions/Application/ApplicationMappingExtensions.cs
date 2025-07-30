using Microsoft.Extensions.DependencyInjection;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Application;

public static class ApplicationMappingExtensions
{
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(OlloLifestyleAPI.Application.Mappings.MasterMappingProfile));

        return services;
    }
}