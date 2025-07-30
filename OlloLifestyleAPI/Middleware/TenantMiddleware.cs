using System.Security.Claims;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        try
        {
            await ResolveTenantAsync(context, tenantService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant for request {RequestPath}", context.Request.Path);
            
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid tenant information");
            return;
        }

        await _next(context);
    }

    private async Task ResolveTenantAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantInfo = await GetTenantFromJwtAsync(context, tenantService);

        if (tenantInfo != null)
        {
            tenantService.SetCurrentTenant(tenantInfo);
            _logger.LogDebug("Tenant resolved: {CompanyId} - {CompanyName}", 
                tenantInfo.CompanyId, tenantInfo.CompanyName);
        }
    }

    private async Task<Application.DTOs.Tenant.TenantInfo?> GetTenantFromJwtAsync(
        HttpContext context, 
        ITenantService tenantService)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            return null;

        // Try to get company_id from the new JWT claims structure
        var companyIdClaim = user.FindFirst("company_id")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId))
        {
            // Fallback to old claim structure for backward compatibility
            companyIdClaim = user.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                return null;
        }

        var tenantInfo = await tenantService.GetTenantAsync(companyId);
        if (tenantInfo == null)
        {
            _logger.LogWarning("Tenant not found for CompanyId: {CompanyId}", companyId);
            return null;
        }

        if (!tenantInfo.IsActive)
        {
            _logger.LogWarning("Inactive tenant accessed: {CompanyId}", companyId);
            throw new UnauthorizedAccessException("Tenant account is inactive");
        }

        return tenantInfo;
    }

}