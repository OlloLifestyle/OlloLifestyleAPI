using OlloLifestyleAPI.Core.Entities.Common;

namespace OlloLifestyleAPI.Core.Entities.Tenant;

public class Order : AuditableEntityGuid
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }

    public virtual Employee? Employee { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}