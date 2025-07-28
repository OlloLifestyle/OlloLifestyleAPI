using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OlloLifestyleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    protected string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    protected IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    protected bool HasPermission(string permission)
    {
        return User.HasClaim("permission", permission);
    }

    protected IActionResult HandleResult<T>(T? result) where T : class
    {
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    protected IActionResult HandleResult<T>(T result, Func<T, bool> successCondition) where T : struct
    {
        if (successCondition(result))
        {
            return Ok(result);
        }

        return BadRequest();
    }
}