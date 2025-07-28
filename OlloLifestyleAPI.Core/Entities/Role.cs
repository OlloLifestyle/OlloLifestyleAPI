using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities;

public class Role : IdentityRole<int>
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}