using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities;

public class User : IdentityUser<int>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();

    public string FullName => $"{FirstName} {LastName}";
}