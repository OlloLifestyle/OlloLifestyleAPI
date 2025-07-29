using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}