using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Application.DTOs.Master;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// User Management DTOs
public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public List<RoleInfo> Roles { get; set; } = new();
    public List<CompanyInfo> Companies { get; set; } = new();
}

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public List<int> RoleIds { get; set; } = new();
    public List<int> CompanyIds { get; set; } = new();
}

public class UpdateUserRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public List<int> RoleIds { get; set; } = new();
    public List<int> CompanyIds { get; set; } = new();
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class RoleInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}