using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DatabaseName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ConnectionString { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}