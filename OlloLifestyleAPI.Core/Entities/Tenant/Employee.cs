using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Tenant;

public class Employee
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EmployeeNumber { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public decimal? Salary { get; set; }

    public DateTime? HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}