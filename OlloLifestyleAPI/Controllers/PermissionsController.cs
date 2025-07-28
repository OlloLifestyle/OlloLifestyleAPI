using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Infrastructure.DbContexts;

namespace OlloLifestyleAPI.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class PermissionsController : BaseController
{
    private readonly AppDbContext _context;

    public PermissionsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permissions</returns>
    [HttpGet]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Module,
                    p.IsActive
                })
                .ToListAsync(cancellationToken);

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get permissions grouped by module
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permissions grouped by module</returns>
    [HttpGet("by-module")]
    public async Task<IActionResult> GetPermissionsByModule(CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .GroupBy(p => p.Module)
                .Select(g => new
                {
                    Module = g.Key,
                    Permissions = g.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}