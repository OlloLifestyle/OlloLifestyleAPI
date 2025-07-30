using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OlloLifestyleAPI.Application.Authorization;
using OlloLifestyleAPI.Application.DTOs.Master;
using OlloLifestyleAPI.Application.Interfaces.Services;
using System.Security.Claims;

namespace OlloLifestyleAPI.Controllers;

/// <summary>
/// Authentication and Authorization Controller
/// Handles login, logout, token refresh, and user session management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserManagementService userManagementService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token with claims
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User {UserName} logged in successfully", request.UserName);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for {UserName}: {Message}", request.UserName, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {UserName}", request.UserName);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.LogoutAsync(userId);
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current authenticated user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userManagementService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    /// <summary>
    /// Change current user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var success = await _userManagementService.ChangePasswordAsync(userId, request);
            
            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Current password may be incorrect." });
            }
            
            _logger.LogInformation("User {UserId} changed password successfully", userId);
            
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user");
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Validate user access to specific company (Multi-tenant)
    /// </summary>
    [HttpGet("validate-company-access")]
    [Authorize]
    public async Task<ActionResult> ValidateCompanyAccess([FromQuery] int companyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var hasAccess = await _authService.ValidateUserAccessToCompanyAsync(userId, companyId);
            
            if (hasAccess)
            {
                return Ok(new { message = "Access granted", companyId });
            }
            
            return Forbid($"User does not have access to company {companyId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating company access for user and company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred during validation" });
        }
    }

    /// <summary>
    /// Get user's permissions (for client-side authorization)
    /// </summary>
    [HttpGet("permissions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetUserPermissions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var permissions = await _userManagementService.GetUserPermissionsAsync(userId);
            
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user permissions");
            return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
        }
    }

    /// <summary>
    /// Get user's accessible companies
    /// </summary>
    [HttpGet("companies")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetUserCompanies()
    {
        try
        {
            var userId = GetCurrentUserId();
            var companies = await _userManagementService.GetUserCompaniesAsync(userId);
            
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user companies");
            return StatusCode(500, new { message = "An error occurred while retrieving companies" });
        }
    }

    /// <summary>
    /// Check if user has specific permission
    /// </summary>
    [HttpGet("check-permission/{permission}")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckPermission(string permission)
    {
        try
        {
            var userId = GetCurrentUserId();
            var hasPermission = await _userManagementService.UserHasPermissionAsync(userId, permission);
            
            return Ok(new { hasPermission, permission });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission}", permission);
            return StatusCode(500, new { message = "An error occurred while checking permission" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> RefreshToken()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userManagementService.GetUserByIdAsync(userId);
            
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "User not found or inactive" });
            }

            // Generate new token with current user data
            var response = await _authService.RefreshTokenAsync(userId);
            
            _logger.LogInformation("Token refreshed for user {UserId}", userId);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { message = "An error occurred while refreshing token" });
        }
    }

    #region Private Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID claim");
        }
        return userId;
    }

    #endregion
}