using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Infrastructure.Extensions.Repository;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddMasterRepositories();
        services.AddTenantRepositories();

        return services;
    }
}