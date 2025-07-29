using Microsoft.Extensions.Logging;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Core.Entities.Tenant;

namespace OlloLifestyleAPI.Application.Services.Tenant;

public class EmployeeService : IEmployeeService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ICompanyRepository companyRepository, ILogger<EmployeeService> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _companyRepository.GetAllEmployeesAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
    {
        var employee = await _companyRepository.GetEmployeeByIdAsync(id);
        return employee != null ? MapToDto(employee) : null;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
    {
        // Check if email already exists
        if (await _companyRepository.EmployeeEmailExistsAsync(createDto.Email))
        {
            throw new InvalidOperationException($"Employee with email {createDto.Email} already exists.");
        }

        // Check if employee number already exists (if provided)
        if (!string.IsNullOrEmpty(createDto.EmployeeNumber) && 
            await _companyRepository.EmployeeNumberExistsAsync(createDto.EmployeeNumber))
        {
            throw new InvalidOperationException($"Employee with number {createDto.EmployeeNumber} already exists.");
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = createDto.Email,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            EmployeeNumber = createDto.EmployeeNumber,
            Department = createDto.Department,
            Position = createDto.Position,
            Salary = createDto.Salary,
            HireDate = createDto.HireDate,
            Phone = createDto.Phone,
            Address = createDto.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdEmployee = await _companyRepository.CreateEmployeeAsync(employee);

        _logger.LogInformation("Created employee {EmployeeId} with email {Email}", createdEmployee.Id, createdEmployee.Email);

        return MapToDto(createdEmployee);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateDto)
    {
        var employee = await _companyRepository.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID {id} not found.");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(updateDto.FirstName))
            employee.FirstName = updateDto.FirstName;
        
        if (!string.IsNullOrEmpty(updateDto.LastName))
            employee.LastName = updateDto.LastName;
        
        if (!string.IsNullOrEmpty(updateDto.Department))
            employee.Department = updateDto.Department;
        
        if (!string.IsNullOrEmpty(updateDto.Position))
            employee.Position = updateDto.Position;
        
        if (updateDto.Salary.HasValue)
            employee.Salary = updateDto.Salary.Value;
        
        if (updateDto.HireDate.HasValue)
            employee.HireDate = updateDto.HireDate.Value;
        
        if (updateDto.IsActive.HasValue)
            employee.IsActive = updateDto.IsActive.Value;
        
        if (!string.IsNullOrEmpty(updateDto.Phone))
            employee.Phone = updateDto.Phone;
        
        if (!string.IsNullOrEmpty(updateDto.Address))
            employee.Address = updateDto.Address;

        employee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _companyRepository.UpdateEmployeeAsync(employee);

        _logger.LogInformation("Updated employee {EmployeeId}", updatedEmployee.Id);

        return MapToDto(updatedEmployee);
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        var success = await _companyRepository.DeleteEmployeeAsync(id);

        if (success)
        {
            _logger.LogInformation("Soft deleted employee {EmployeeId}", id);
        }

        return success;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
    {
        var employees = await _companyRepository.GetEmployeesByDepartmentAsync(department);
        return employees.Select(MapToDto);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            EmployeeNumber = employee.EmployeeNumber,
            Department = employee.Department,
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            Phone = employee.Phone,
            Address = employee.Address,
            CreatedAt = employee.CreatedAt
        };
    }
}