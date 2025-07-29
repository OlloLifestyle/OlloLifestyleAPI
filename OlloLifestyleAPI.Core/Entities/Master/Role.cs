using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public bool IsSystemRole { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}