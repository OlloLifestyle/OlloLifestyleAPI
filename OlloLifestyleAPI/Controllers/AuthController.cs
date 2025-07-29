using Microsoft.AspNetCore.Mvc;
using OlloLifestyleAPI.Application.DTOs.Master;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
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

    [HttpGet("validate-company-access")]
    public async Task<ActionResult> ValidateCompanyAccess(int userId, int companyId)
    {
        try
        {
            var hasAccess = await _authService.ValidateUserAccessToCompanyAsync(userId, companyId);
            
            if (hasAccess)
            {
                return Ok(new { message = "Access granted" });
            }
            
            return Forbid("User does not have access to this company");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating company access for user {UserId} and company {CompanyId}", 
                userId, companyId);
            return StatusCode(500, new { message = "An error occurred during validation" });
        }
    }
}