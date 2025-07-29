using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Tenant;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders.Tenant;

public static class OrdersSeed
{
    public static async Task SeedAsync(CompanyDbContext context)
    {
        if (await context.Orders.AnyAsync())
            return;

        var employees = await context.Employees.ToListAsync();
        var products = await context.Products.ToListAsync();

        if (!employees.Any() || !products.Any())
            return;

        var orders = new[]
        {
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD001",
                EmployeeId = employees[0].Id,
                CustomerName = "ABC Company",
                CustomerEmail = "orders@abccompany.com",
                TotalAmount = 1599.98m,
                OrderDate = DateTime.UtcNow.AddDays(-5),
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD002",
                EmployeeId = employees[1].Id,
                CustomerName = "XYZ Corporation",
                CustomerEmail = "purchasing@xyzcorp.com",
                TotalAmount = 329.98m,
                OrderDate = DateTime.UtcNow.AddDays(-3),
                Status = "Processing",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        // Create order items
        var orderItems = new[]
        {
            new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orders[0].Id,
                ProductId = products[0].Id,
                Quantity = 1,
                UnitPrice = products[0].Price,
                TotalPrice = products[0].Price,
                CreatedAt = DateTime.UtcNow
            },
            new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orders[0].Id,
                ProductId = products[1].Id,
                Quantity = 1,
                UnitPrice = products[1].Price,
                TotalPrice = products[1].Price,
                CreatedAt = DateTime.UtcNow
            },
            new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orders[1].Id,
                ProductId = products[1].Id,
                Quantity = 1,
                UnitPrice = products[1].Price,
                TotalPrice = products[1].Price,
                CreatedAt = DateTime.UtcNow
            },
            new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orders[1].Id,
                ProductId = products[2].Id,
                Quantity = 1,
                UnitPrice = products[2].Price,
                TotalPrice = products[2].Price,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();
    }
}