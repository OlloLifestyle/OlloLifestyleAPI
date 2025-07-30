using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Validators.FactoryFlowTracker;

namespace OlloLifestyleAPI.Infrastructure.Extensions.FactoryFlowTracker;

public static class FactoryFlowTrackerValidationExtensions
{
    public static IServiceCollection AddFactoryFlowTrackerValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateWorkOrderRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProductionLineRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<QualityCheckRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<InventoryItemRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ProductionScheduleRequestValidator>();

        return services;
    }
}