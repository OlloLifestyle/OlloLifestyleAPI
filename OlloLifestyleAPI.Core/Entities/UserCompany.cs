namespace OlloLifestyleAPI.Core.Entities;

public class UserCompany : BaseEntity
{
    public int UserId { get; set; }
    
    public int CompanyId { get; set; }

    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public string? AssignedBy { get; set; }

    public virtual User User { get; set; } = null!;
    
    public virtual Company Company { get; set; } = null!;
}