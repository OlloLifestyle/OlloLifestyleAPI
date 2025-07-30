using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Mappings.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

public static class FactoryFlowTrackerMappingExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(FactoryFlowTrackerMappingProfile));

        return services;
    }
}