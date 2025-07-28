using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}