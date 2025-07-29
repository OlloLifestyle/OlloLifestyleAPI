using OlloLifestyleAPI.Application.DTOs.Tenant;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface ITenantService
{
    Task<TenantInfo?> GetTenantAsync(int companyId);
    TenantInfo? GetCurrentTenant();
    void SetCurrentTenant(TenantInfo tenantInfo);
    void ClearCurrentTenant();
}