namespace OlloLifestyleAPI.Application.DTOs.Master;

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<int> CompanyIds { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<int> CompanyIds { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<CompanyInfo> Companies { get; set; } = new();
    public List<RoleInfo> Roles { get; set; } = new();
}

public class RoleInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}