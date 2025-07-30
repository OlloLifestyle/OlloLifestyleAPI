using Microsoft.AspNetCore.Authorization;

namespace OlloLifestyleAPI.Application.Authorization;

/// <summary>
/// Attribute to require specific permission for accessing an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base($"Permission.{permission}")
    {
    }
}

/// <summary>
/// Attribute to require specific role for accessing an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles) : base($"Role.{string.Join(",", roles)}")
    {
    }
}

/// <summary>
/// Attribute to require access to specific company
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireCompanyAccessAttribute : AuthorizeAttribute
{
    public RequireCompanyAccessAttribute() : base("CompanyAccess")
    {
    }
}

/// <summary>
/// Attribute to allow only system administrators
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireSystemAdminAttribute : RequireRoleAttribute
{
    public RequireSystemAdminAttribute() : base("SystemAdmin")
    {
    }
}

/// <summary>
/// Attribute to allow system administrators and company administrators
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAdminAttribute : RequireRoleAttribute
{
    public RequireAdminAttribute() : base("SystemAdmin", "CompanyAdmin")
    {
    }
}