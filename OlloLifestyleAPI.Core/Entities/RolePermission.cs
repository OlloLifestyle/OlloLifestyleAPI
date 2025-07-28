namespace OlloLifestyleAPI.Core.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    
    public int PermissionId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public string? AssignedBy { get; set; }

    public virtual Role Role { get; set; } = null!;
    
    public virtual Permission Permission { get; set; } = null!;
}