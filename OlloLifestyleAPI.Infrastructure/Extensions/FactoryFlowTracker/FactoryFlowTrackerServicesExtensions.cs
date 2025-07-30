using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Services.FactoryFlowTracker;
using OlloLifestyleAPI.Application.Services.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

public static class FactoryFlowTrackerServicesExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IProductionLineService, ProductionLineService>();
        services.AddScoped<IQualityControlService, QualityControlService>();
        services.AddScoped<IInventoryManagementService, InventoryManagementService>();
        services.AddScoped<IProductionSchedulingService, ProductionSchedulingService>();
        services.AddScoped<IFactoryReportingService, FactoryReportingService>();

        return services;
    }
}