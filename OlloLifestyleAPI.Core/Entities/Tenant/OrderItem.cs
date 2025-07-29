using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.Entities.Tenant;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}