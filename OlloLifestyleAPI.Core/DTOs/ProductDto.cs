using System.ComponentModel.DataAnnotations;

namespace OlloLifestyleAPI.Core.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public string? Category { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

public record CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    [MaxLength(100)]
    public string? Category { get; init; }
}

public record UpdateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public bool IsActive { get; init; } = true;

    [MaxLength(100)]
    public string? Category { get; init; }
}