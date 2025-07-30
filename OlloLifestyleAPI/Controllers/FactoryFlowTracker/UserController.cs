using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OlloLifestyleAPI.Application.Authorization;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Application.Interfaces.Services;
using System.Security.Claims;

namespace OlloLifestyleAPI.Controllers.FactoryFlowTracker;

/// <summary>
/// Factory Flow Tracker User Management Controller
/// Handles CRUD operations for factory floor users with proper authorization
/// </summary>
[ApiController]
[Route("api/factory-flow-tracker/[controller]")]
[Produces("application/json")]
[Authorize]
[RequireCompanyAccess]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users with optional filtering
    /// </summary>
    [HttpGet]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<UserListResponse>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? department = null,
        [FromQuery] int? status = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var response = await _userService.GetUsersAsync(pageNumber, pageSize, searchTerm, department, status);
            
            _logger.LogInformation("Retrieved {Count} users (Page {PageNumber}/{TotalPages})", 
                response.Users.Count(), pageNumber, response.TotalPages);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with filters: searchTerm={SearchTerm}, department={Department}, status={Status}", 
                searchTerm, department, status);
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<FactoryFlowTrackerUserDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    [HttpGet("by-email/{email}")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<FactoryFlowTrackerUserDto>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with email {email} not found" });
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Get users by department
    /// </summary>
    [HttpGet("by-department/{department}")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<IEnumerable<FactoryFlowTrackerUserDto>>> GetUsersByDepartment(string department)
    {
        try
        {
            var users = await _userService.GetUsersByDepartmentAsync(department);
            
            _logger.LogInformation("Retrieved {Count} users for department {Department}", users.Count(), department);
            
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for department {Department}", department);
            return StatusCode(500, new { message = "An error occurred while retrieving users by department" });
        }
    }

    /// <summary>
    /// Get only active users
    /// </summary>
    [HttpGet("active")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<IEnumerable<FactoryFlowTrackerUserDto>>> GetActiveUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            
            _logger.LogInformation("Retrieved {Count} active users", users.Count());
            
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            return StatusCode(500, new { message = "An error occurred while retrieving active users" });
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [RequirePermission("factoryflowtracker.user.create")]
    public async Task<ActionResult<FactoryFlowTrackerUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(request);
            
            _logger.LogInformation("User created successfully with ID {UserId} by {CreatedBy}", 
                user.Id, GetCurrentUserName());
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid user creation request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("factoryflowtracker.user.update")]
    public async Task<ActionResult<FactoryFlowTrackerUserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateUserAsync(id, request);
            
            _logger.LogInformation("User {UserId} updated successfully by {UpdatedBy}", 
                id, GetCurrentUserName());
            
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid user update request for {UserId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("factoryflowtracker.user.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            
            _logger.LogInformation("User {UserId} deleted successfully by {DeletedBy}", 
                id, GetCurrentUserName());
            
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    /// <summary>
    /// Activate user
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [RequirePermission("factoryflowtracker.user.update")]
    public async Task<ActionResult> ActivateUser(Guid id)
    {
        try
        {
            var success = await _userService.ActivateUserAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            
            _logger.LogInformation("User {UserId} activated by {ActivatedBy}", id, GetCurrentUserName());
            
            return Ok(new { message = "User activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while activating the user" });
        }
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [RequirePermission("factoryflowtracker.user.update")]
    public async Task<ActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var success = await _userService.DeactivateUserAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            
            _logger.LogInformation("User {UserId} deactivated by {DeactivatedBy}", id, GetCurrentUserName());
            
            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating the user" });
        }
    }

    /// <summary>
    /// Suspend user
    /// </summary>
    [HttpPatch("{id:guid}/suspend")]
    [RequirePermission("factoryflowtracker.user.suspend")]
    public async Task<ActionResult> SuspendUser(Guid id)
    {
        try
        {
            var success = await _userService.SuspendUserAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            
            _logger.LogInformation("User {UserId} suspended by {SuspendedBy}", id, GetCurrentUserName());
            
            return Ok(new { message = "User suspended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while suspending the user" });
        }
    }

    #region Lookup Data Endpoints

    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet("departments")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<IEnumerable<string>>> GetDepartments()
    {
        try
        {
            var departments = await _userService.GetDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, new { message = "An error occurred while retrieving departments" });
        }
    }

    /// <summary>
    /// Get all positions
    /// </summary>
    [HttpGet("positions")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<IEnumerable<string>>> GetPositions()
    {
        try
        {
            var positions = await _userService.GetPositionsAsync();
            return Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving positions");
            return StatusCode(500, new { message = "An error occurred while retrieving positions" });
        }
    }

    /// <summary>
    /// Get positions by department
    /// </summary>
    [HttpGet("departments/{department}/positions")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<IEnumerable<string>>> GetPositionsByDepartment(string department)
    {
        try
        {
            var positions = await _userService.GetPositionsByDepartmentAsync(department);
            return Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving positions for department {Department}", department);
            return StatusCode(500, new { message = "An error occurred while retrieving positions" });
        }
    }

    #endregion

    #region Statistics Endpoints

    /// <summary>
    /// Get user statistics dashboard
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult> GetUserStatistics()
    {
        try
        {
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var activeUsers = await _userService.GetActiveUsersCountAsync();
            var usersByDepartment = await _userService.GetUsersByDepartmentCountAsync();
            var usersByStatus = await _userService.GetUsersByStatusCountAsync();

            var statistics = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                UsersByDepartment = usersByDepartment,
                UsersByStatus = usersByStatus
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get total users count
    /// </summary>
    [HttpGet("count")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<int>> GetTotalUsersCount()
    {
        try
        {
            var count = await _userService.GetTotalUsersCountAsync();
            return Ok(new { totalUsers = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total users count");
            return StatusCode(500, new { message = "An error occurred while retrieving user count" });
        }
    }

    /// <summary>
    /// Get active users count
    /// </summary>
    [HttpGet("count/active")]
    [RequirePermission("factoryflowtracker.user.read")]
    public async Task<ActionResult<int>> GetActiveUsersCount()
    {
        try
        {
            var count = await _userService.GetActiveUsersCountAsync();
            return Ok(new { activeUsers = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users count");
            return StatusCode(500, new { message = "An error occurred while retrieving active user count" });
        }
    }

    #endregion

    #region Private Methods

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID claim");
        }
        return userId;
    }

    private int GetCurrentCompanyId()
    {
        var companyClaim = User.FindFirst("company")?.Value;
        if (string.IsNullOrEmpty(companyClaim) || !int.TryParse(companyClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("Invalid company claim");
        }
        return companyId;
    }

    #endregion
}