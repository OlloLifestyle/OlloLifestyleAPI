using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence.FactoryFlowTracker;
using OlloLifestyleAPI.Infrastructure.Persistence.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

public static class FactoryFlowTrackerUnitOfWorkExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IFactoryFlowTrackerUnitOfWork, FactoryFlowTrackerUnitOfWork>();

        return services;
    }
}