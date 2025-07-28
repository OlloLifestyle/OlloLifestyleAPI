namespace OlloLifestyleAPI.Core.DTOs;

public record CompanyDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsDefault { get; init; }
}