namespace OlloLifestyleAPI.Application.DTOs.Tenant;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class UpdateEmployeeDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public bool? IsActive { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}