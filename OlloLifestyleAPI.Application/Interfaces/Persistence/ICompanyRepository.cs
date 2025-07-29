using OlloLifestyleAPI.Core.Entities.Tenant;

namespace OlloLifestyleAPI.Application.Interfaces.Persistence;

public interface ICompanyRepository
{
    // Employee operations
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<Employee> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department);
    Task<bool> EmployeeEmailExistsAsync(string email);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);

    // Order operations (can be extended later)
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(Guid id);
}