using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.Tenant;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;

namespace OlloLifestyleAPI.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly CompanyDbFactory _companyDbFactory;

    public CompanyRepository(CompanyDbFactory companyDbFactory)
    {
        _companyDbFactory = companyDbFactory;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
        
        return employee;
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return false;
        }

        // Soft delete
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Employees
            .Where(e => e.Department == department && e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<bool> EmployeeEmailExistsAsync(string email)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Employees
            .AnyAsync(e => e.Email == email);
    }

    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Employees
            .AnyAsync(e => e.EmployeeNumber == employeeNumber);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Orders
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Orders
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}