using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OlloLifestyleAPI.Application.Authorization;

namespace OlloLifestyleAPI.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var (scope, action) = PermissionClaimHelper.ParsePermission(_permission);
        if (action is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var claimType = PermissionClaimHelper.GetClaimType(scope);
        var hasPermission = user.Claims
            .Where(c => c.Type == claimType)
            .SelectMany(c => PermissionClaimHelper.ExtractActionsFromClaimValue(c.Value))
            .Any(a => a == action);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasRole = _roles.Any(role => user.IsInRole(role));

        if (!hasRole)
        {
            context.Result = new ForbidResult();
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCompanyAccessAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if user has company access
        var hasCompanyAccess = user.Claims.Any(c => c.Type == "company_id");

        if (!hasCompanyAccess)
        {
            context.Result = new ForbidResult();
        }
    }
}
