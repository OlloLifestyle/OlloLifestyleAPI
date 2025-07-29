namespace OlloLifestyleAPI.Application.DTOs.Tenant;

public class TenantInfo
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}