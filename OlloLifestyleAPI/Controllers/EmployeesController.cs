using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication, which triggers tenant resolution
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IEmployeeService employeeService,
        ITenantService tenantService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        try
        {
            var currentTenant = _tenantService.GetCurrentTenant();
            _logger.LogInformation("Getting employees for tenant {CompanyId}", currentTenant?.CompanyId);

            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} not found");
            }

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee");
        }
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await _employeeService.CreateEmployeeAsync(createDto);
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "An error occurred while creating the employee");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await _employeeService.UpdateEmployeeAsync(id, updateDto);
            return Ok(employee);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while updating the employee");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEmployee(Guid id)
    {
        try
        {
            var success = await _employeeService.DeleteEmployeeAsync(id);
            
            if (!success)
            {
                return NotFound($"Employee with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while deleting the employee");
        }
    }

    [HttpGet("department/{department}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartment(string department)
    {
        try
        {
            var employees = await _employeeService.GetEmployeesByDepartmentAsync(department);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees for department {Department}", department);
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    [HttpGet("tenant-info")]
    public ActionResult GetCurrentTenantInfo()
    {
        var currentTenant = _tenantService.GetCurrentTenant();
        
        if (currentTenant == null)
        {
            return BadRequest("No tenant context found");
        }

        return Ok(new
        {
            CompanyId = currentTenant.CompanyId,
            CompanyName = currentTenant.CompanyName,
            DatabaseName = currentTenant.DatabaseName,
            IsActive = currentTenant.IsActive
        });
    }
}