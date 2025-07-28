using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities;

public class Company : AuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ConnectionString { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}