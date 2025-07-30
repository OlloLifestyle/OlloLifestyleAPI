using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Repositories.Master;
using OlloLifestyleAPI.Infrastructure.Repositories.Tenant;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Auth;

public static class AuthRepositoryExtensions
{
    public static IServiceCollection AddAuthRepositories(this IServiceCollection services)
    {
        // Master repositories (for authentication)
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IMasterUnitOfWork>());
        
        // Tenant repositories (for business data)
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }
}