using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Application.DTOs.Master;

// Permission Management DTOs
public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePermissionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
}

public class UpdatePermissionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class PermissionsByModuleDto
{
    public string Module { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}