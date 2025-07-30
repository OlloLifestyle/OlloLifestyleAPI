using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Application;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, OlloLifestyleAPI.Application.Services.Tenant.EmployeeService>();

        return services;
    }
}