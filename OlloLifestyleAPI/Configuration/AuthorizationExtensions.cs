using Microsoft.AspNetCore.Authorization;
using OlloLifestyleAPI.Application.Authorization;

namespace OlloLifestyleAPI.Configuration;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            ConfigurePermissionPolicies(options);
            ConfigureRolePolicies(options);
            ConfigureCompanyAccessPolicies(options);
        });

        RegisterAuthorizationHandlers(services);
        return services;
    }

    private static void ConfigurePermissionPolicies(AuthorizationOptions options)
    {
        // User management permissions
        options.AddPolicy("Permission.factoryflowtracker.user.read", policy =>
            policy.Requirements.Add(new PermissionRequirement("factoryflowtracker.user.read")));
        
        options.AddPolicy("Permission.factoryflowtracker.user.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("factoryflowtracker.user.create")));
        
        options.AddPolicy("Permission.factoryflowtracker.user.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("factoryflowtracker.user.update")));
        
        options.AddPolicy("Permission.factoryflowtracker.user.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("factoryflowtracker.user.delete")));
        
        options.AddPolicy("Permission.factoryflowtracker.user.suspend", policy =>
            policy.Requirements.Add(new PermissionRequirement("factoryflowtracker.user.suspend")));

        // Employee management permissions
        options.AddPolicy("Permission.employee.read", policy =>
            policy.Requirements.Add(new PermissionRequirement("employee.read")));
        
        options.AddPolicy("Permission.employee.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("employee.create")));
        
        options.AddPolicy("Permission.employee.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("employee.update")));
        
        options.AddPolicy("Permission.employee.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("employee.delete")));

        // Product management permissions
        options.AddPolicy("Permission.product.read", policy =>
            policy.Requirements.Add(new PermissionRequirement("product.read")));
        
        options.AddPolicy("Permission.product.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("product.create")));
        
        options.AddPolicy("Permission.product.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("product.update")));
        
        options.AddPolicy("Permission.product.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("product.delete")));

        // Order management permissions
        options.AddPolicy("Permission.order.read", policy =>
            policy.Requirements.Add(new PermissionRequirement("order.read")));
        
        options.AddPolicy("Permission.order.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("order.create")));
        
        options.AddPolicy("Permission.order.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("order.update")));
        
        options.AddPolicy("Permission.order.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("order.delete")));
    }

    private static void ConfigureRolePolicies(AuthorizationOptions options)
    {
        options.AddPolicy("Role.Administrator", policy =>
            policy.Requirements.Add(new RoleRequirement("Administrator")));
        
        options.AddPolicy("Role.SystemAdmin", policy =>
            policy.Requirements.Add(new RoleRequirement("SystemAdmin")));
        
        options.AddPolicy("Role.Manager", policy =>
            policy.Requirements.Add(new RoleRequirement("Manager")));
        
        options.AddPolicy("Role.Employee", policy =>
            policy.Requirements.Add(new RoleRequirement("Employee")));
        
        options.AddPolicy("Role.User", policy =>
            policy.Requirements.Add(new RoleRequirement("User")));
    }

    private static void ConfigureCompanyAccessPolicies(AuthorizationOptions options)
    {
        options.AddPolicy("CompanyAccess", policy =>
            policy.Requirements.Add(new CompanyAccessRequirement(0))); // 0 means any company
    }

    private static void RegisterAuthorizationHandlers(IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, CompanyAccessAuthorizationHandler>();
    }
}