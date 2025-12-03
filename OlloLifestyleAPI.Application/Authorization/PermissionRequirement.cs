using Microsoft.AspNetCore.Authorization;

namespace OlloLifestyleAPI.Application.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public string Scope { get; }
    public string? Action { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
        (Scope, Action) = PermissionClaimHelper.ParsePermission(permission);
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (requirement.Action is null)
        {
            return Task.CompletedTask;
        }

        var claimType = PermissionClaimHelper.GetClaimType(requirement.Scope);
        var permittedActions = context.User.Claims
            .Where(c => c.Type == claimType)
            .SelectMany(c => PermissionClaimHelper.ExtractActionsFromClaimValue(c.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (permittedActions.Contains(requirement.Action))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class RoleRequirement : IAuthorizationRequirement
{
    public string[] Roles { get; }

    public RoleRequirement(params string[] roles)
    {
        Roles = roles;
    }
}

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var userRoles = context.User.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        if (requirement.Roles.Any(role => userRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class CompanyAccessRequirement : IAuthorizationRequirement
{
    public int CompanyId { get; }

    public CompanyAccessRequirement(int companyId)
    {
        CompanyId = companyId;
    }
}

public class CompanyAccessAuthorizationHandler : AuthorizationHandler<CompanyAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyAccessRequirement requirement)
    {
        var userCompanies = context.User.Claims
            .Where(c => c.Type == "company")
            .Select(c => int.Parse(c.Value))
            .ToList();

        if (userCompanies.Contains(requirement.CompanyId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
