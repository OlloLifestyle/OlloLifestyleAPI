using Microsoft.AspNetCore.Identity;

namespace OlloLifestyleAPI.Core.Entities;

public class UserRole : IdentityUserRole<int>
{
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public string? AssignedBy { get; set; }

    public virtual User User { get; set; } = null!;
    
    public virtual Role Role { get; set; } = null!;
}