using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;
using OlloLifestyleAPI.Infrastructure.Repositories;
using OlloLifestyleAPI.Infrastructure.Services.Tenant;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add AppDbContext (Shared Identity Database)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Add Multi-tenancy services
        services.AddMemoryCache();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<CompanyDbFactory>();

        // Add Repository services
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Authentication services
        services.AddScoped<IAuthService, Infrastructure.Services.Master.AuthService>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(OlloLifestyleAPI.Application.Mappings.MasterMappingProfile));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(OlloLifestyleAPI.Application.Validators.Master.LoginRequestValidator).Assembly);

        // Add application services
        services.AddScoped<IEmployeeService, OlloLifestyleAPI.Application.Services.Tenant.EmployeeService>();
        
        return services;
    }
}