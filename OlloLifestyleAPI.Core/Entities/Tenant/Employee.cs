using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.Tenant;

public class Employee : AuditableEntityGuid
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}