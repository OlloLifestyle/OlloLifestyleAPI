using OlloLifestyleAPI.Core.Interfaces;

namespace OlloLifestyleAPI.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private int? _currentTenantId;
    private string? _currentTenantConnectionString;

    public int? GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public string? GetCurrentTenantConnectionString()
    {
        return _currentTenantConnectionString;
    }

    public void SetTenant(int tenantId, string connectionString)
    {
        _currentTenantId = tenantId;
        _currentTenantConnectionString = connectionString;
    }
}