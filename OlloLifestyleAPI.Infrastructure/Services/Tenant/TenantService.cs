using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Services.Tenant;

public class TenantService : ITenantService
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TenantCacheKeyPrefix = "tenant_";
    private const string TenantDomainCacheKeyPrefix = "tenant_domain_";
    private const int CacheExpirationMinutes = 30;

    public TenantService(
        AppDbContext appDbContext,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor)
    {
        _appDbContext = appDbContext;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TenantInfo?> GetTenantAsync(int companyId)
    {
        var cacheKey = $"{TenantCacheKeyPrefix}{companyId}";
        
        if (_cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
        {
            return cachedTenant;
        }

        var company = await _appDbContext.Companies
            .Where(c => c.Id == companyId && c.IsActive)
            .FirstOrDefaultAsync();

        if (company == null)
            return null;

        var tenantInfo = new TenantInfo
        {
            CompanyId = company.Id,
            CompanyName = company.Name,
            DatabaseName = company.DatabaseName,
            ConnectionString = company.ConnectionString,
            IsActive = company.IsActive
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        _cache.Set(cacheKey, tenantInfo, cacheOptions);
        
        return tenantInfo;
    }


    public TenantInfo? GetCurrentTenant()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Items["TenantInfo"] as TenantInfo;
    }

    public void SetCurrentTenant(TenantInfo tenantInfo)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items["TenantInfo"] = tenantInfo;
        }
    }

    public void ClearCurrentTenant()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items.Remove("TenantInfo");
        }
    }
}