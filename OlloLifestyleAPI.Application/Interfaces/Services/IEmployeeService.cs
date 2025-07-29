using OlloLifestyleAPI.Application.DTOs.Tenant;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto);
    Task<EmployeeDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateDto);
    Task<bool> DeleteEmployeeAsync(Guid id);
    Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
}