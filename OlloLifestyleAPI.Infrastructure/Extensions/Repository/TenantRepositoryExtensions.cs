using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Repository;

public static class TenantRepositoryExtensions
{
    public static IServiceCollection AddTenantRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITenantUnitOfWork>(provider =>
        {
            var factory = provider.GetRequiredService<CompanyDbFactory>();
            var context = factory.CreateDbContext();
            return new TenantUnitOfWork(context);
        });

        return services;
    }
}