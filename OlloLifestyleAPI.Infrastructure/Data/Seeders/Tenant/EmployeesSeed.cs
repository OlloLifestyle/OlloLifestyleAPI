using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Tenant;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders.Tenant;

public static class EmployeesSeed
{
    public static async Task SeedAsync(CompanyDbContext context)
    {
        if (await context.Employees.AnyAsync())
            return;

        var employees = new[]
        {
            new Employee
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@company.com",
                FirstName = "John",
                LastName = "Doe",
                EmployeeNumber = "EMP001",
                Department = "IT",
                Position = "Software Developer",
                Salary = 75000m,
                HireDate = DateTime.UtcNow.AddYears(-2),
                IsActive = true,
                Phone = "+1-555-0101",
                Address = "123 Main St, City, State",
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Email = "jane.smith@company.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmployeeNumber = "EMP002",
                Department = "HR",
                Position = "HR Manager",
                Salary = 65000m,
                HireDate = DateTime.UtcNow.AddYears(-3),
                IsActive = true,
                Phone = "+1-555-0102",
                Address = "456 Oak Ave, City, State",
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Email = "bob.johnson@company.com",
                FirstName = "Bob",
                LastName = "Johnson",
                EmployeeNumber = "EMP003",
                Department = "Sales",
                Position = "Sales Representative",
                Salary = 55000m,
                HireDate = DateTime.UtcNow.AddYears(-1),
                IsActive = true,
                Phone = "+1-555-0103",
                Address = "789 Pine St, City, State",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }
}