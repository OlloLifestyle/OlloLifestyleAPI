using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Tenant;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders.Tenant;

public static class ProductsSeed
{
    public static async Task SeedAsync(CompanyDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var products = new[]
        {
            new Product 
            { 
                Id = Guid.NewGuid(), 
                Name = "Laptop", 
                Description = "High-performance business laptop",
                Sku = "LAP001",
                Price = 1299.99m,
                StockQuantity = 50,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Id = Guid.NewGuid(), 
                Name = "Office Chair", 
                Description = "Ergonomic office chair",
                Sku = "CHR001",
                Price = 299.99m,
                StockQuantity = 25,
                Category = "Furniture",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Id = Guid.NewGuid(), 
                Name = "Wireless Mouse", 
                Description = "Wireless optical mouse",
                Sku = "MOU001",
                Price = 29.99m,
                StockQuantity = 100,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}