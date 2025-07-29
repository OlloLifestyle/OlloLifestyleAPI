using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OlloLifestyleAPI.Core.Interfaces;
using System.Security.Claims;

namespace OlloLifestyleAPI.Infrastructure.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditFields(DbContext context)
    {
        var currentUser = GetCurrentUser();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedAt = utcNow;
                        auditableEntity.CreatedBy = currentUser;
                        break;

                    case EntityState.Modified:
                        auditableEntity.UpdatedAt = utcNow;
                        auditableEntity.UpdatedBy = currentUser;
                        // Prevent overwriting CreatedAt and CreatedBy
                        entry.Property(nameof(auditableEntity.CreatedAt)).IsModified = false;
                        entry.Property(nameof(auditableEntity.CreatedBy)).IsModified = false;
                        break;
                }
            }

            if (entry.Entity is ISoftDeletable softDeletableEntity && entry.State == EntityState.Deleted)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                softDeletableEntity.IsDeleted = true;
                softDeletableEntity.DeletedAt = utcNow;
                softDeletableEntity.DeletedBy = currentUser;
            }
        }
    }

    private string? GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value 
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? "System";
        }

        return "System";
    }
}