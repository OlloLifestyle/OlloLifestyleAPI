using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Infrastructure.Persistence;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;
using OlloLifestyleAPI.Infrastructure.Repositories.Master;
using OlloLifestyleAPI.Infrastructure.Repositories.Tenant;
using OlloLifestyleAPI.Infrastructure.Services.Tenant;
using OlloLifestyleAPI.Infrastructure.Interceptors;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add interceptors
        services.AddScoped<AuditInterceptor>();

        // Add AppDbContext (Shared Identity Database)
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

        // Add Multi-tenancy services
        services.AddMemoryCache();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<CompanyDbFactory>();

        // Add Repository services - Master context
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IMasterUnitOfWork>());

        // Add Repository services - Tenant context
        services.AddScoped<ITenantUnitOfWork>(provider =>
        {
            var factory = provider.GetRequiredService<CompanyDbFactory>();
            var context = factory.CreateDbContext();
            return new TenantUnitOfWork(context);
        });
        services.AddScoped<ICompanyRepository, CompanyRepository>();

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