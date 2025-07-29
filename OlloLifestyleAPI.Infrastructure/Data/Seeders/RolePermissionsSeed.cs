using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Data.Seeders;

public static class RolePermissionsSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");
        var managerRole = await context.Roles.FirstAsync(r => r.Name == "Manager");
        var employeeRole = await context.Roles.FirstAsync(r => r.Name == "Employee");
        var permissions = await context.Permissions.ToListAsync();

        var rolePermissions = new List<RolePermission>();

        // Admin gets all permissions
        foreach (var permission in permissions)
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = adminRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        // Manager gets read/write permissions (no delete)
        foreach (var permission in permissions.Where(p => p.Action != "Delete"))
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = managerRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        // Employee gets read permissions only
        foreach (var permission in permissions.Where(p => p.Action == "Read"))
        {
            rolePermissions.Add(new RolePermission 
            { 
                RoleId = employeeRole.Id, 
                PermissionId = permission.Id, 
                AssignedAt = DateTime.UtcNow 
            });
        }

        context.RolePermissions.AddRange(rolePermissions);
        await context.SaveChangesAsync();
    }
}