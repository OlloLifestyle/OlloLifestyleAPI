namespace OlloLifestyleAPI.Core.Entities.Common;

public abstract class AuditableEntityGuid : BaseEntityGuid
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}