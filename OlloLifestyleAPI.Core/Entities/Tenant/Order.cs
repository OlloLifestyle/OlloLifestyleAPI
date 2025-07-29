using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Tenant;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}