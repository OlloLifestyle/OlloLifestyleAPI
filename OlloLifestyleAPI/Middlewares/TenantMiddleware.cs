using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Interfaces;
using OlloLifestyleAPI.Infrastructure.DbContexts;
using System.Security.Claims;

namespace OlloLifestyleAPI.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, AppDbContext appDbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                var userCompany = await appDbContext.UserCompanies
                    .Include(uc => uc.Company)
                    .Where(uc => uc.UserId == userId && uc.IsActive && uc.Company.IsActive)
                    .OrderByDescending(uc => uc.IsDefault)
                    .FirstOrDefaultAsync();

                if (userCompany != null)
                {
                    tenantProvider.SetTenant(userCompany.CompanyId, userCompany.Company.ConnectionString);
                    
                    context.Items["TenantId"] = userCompany.CompanyId;
                    context.Items["CompanyName"] = userCompany.Company.Name;
                }
            }
        }

        await _next(context);
    }
}