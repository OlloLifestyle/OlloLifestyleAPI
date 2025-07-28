namespace OlloLifestyleAPI.Core.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public string FullName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public List<CompanyDto> Companies { get; init; } = new();
}