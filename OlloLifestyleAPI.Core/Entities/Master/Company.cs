using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}