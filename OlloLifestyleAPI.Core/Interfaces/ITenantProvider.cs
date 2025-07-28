namespace OlloLifestyleAPI.Core.Interfaces;

public interface ITenantProvider
{
    int? GetCurrentTenantId();
    string? GetCurrentTenantConnectionString();
    void SetTenant(int tenantId, string connectionString);
}