using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence.FactoryFlowTracker;
using OlloLifestyleAPI.Infrastructure.Repositories.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

public static class FactoryFlowTrackerRepositoryExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IProductionLineRepository, ProductionLineRepository>();
        services.AddScoped<IQualityCheckRepository, QualityCheckRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IProductionScheduleRepository, ProductionScheduleRepository>();

        return services;
    }
}