using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Repositories.Master;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Repository;

public static class MasterRepositoryExtensions
{
    public static IServiceCollection AddMasterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IMasterUnitOfWork>());
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }
}