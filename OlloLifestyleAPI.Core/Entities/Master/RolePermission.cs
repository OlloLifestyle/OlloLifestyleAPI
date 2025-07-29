using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class RolePermission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PermissionId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual Role Role { get; set; } = null!;
    
    public virtual Permission Permission { get; set; } = null!;
}