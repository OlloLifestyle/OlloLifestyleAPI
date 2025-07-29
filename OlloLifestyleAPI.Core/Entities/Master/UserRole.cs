using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Master;

public class UserRole
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    
    public virtual Role Role { get; set; } = null!;
}