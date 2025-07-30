using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Auth;

public static class AuthServicesExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        // Authentication Services
        services.AddScoped<IAuthService, OlloLifestyleAPI.Infrastructure.Services.Master.AuthService>();
        
        // User Management Services (Master - for auth users)
        services.AddScoped<IUserManagementService, OlloLifestyleAPI.Infrastructure.Services.Master.UserManagementService>();
        // services.AddScoped<IRoleManagementService, OlloLifestyleAPI.Infrastructure.Services.Master.RoleManagementService>();
        // services.AddScoped<IPermissionManagementService, OlloLifestyleAPI.Infrastructure.Services.Master.PermissionManagementService>();
        
        // Tenant Services
        services.AddScoped<ITenantService, OlloLifestyleAPI.Infrastructure.Services.Tenant.TenantService>();
        services.AddScoped<IUserService, OlloLifestyleAPI.Application.Services.Tenant.UserService>();

        return services;
    }
}