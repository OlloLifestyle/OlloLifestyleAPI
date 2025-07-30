using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class FactoryFlowTrackerServiceExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerModule(this IServiceCollection services)
    {
        services.AddFactoryFlowTrackerRepositories();
        services.AddFactoryFlowTrackerUnitOfWork();
        services.AddFactoryFlowTrackerServices();
        services.AddFactoryFlowTrackerMappings();
        services.AddFactoryFlowTrackerValidation();

        return services;
    }
}