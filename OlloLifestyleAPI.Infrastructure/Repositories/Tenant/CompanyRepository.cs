using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;
using OlloLifestyleAPI.Infrastructure.Persistence.Factories;

namespace OlloLifestyleAPI.Infrastructure.Repositories.Tenant;

public class CompanyRepository : ICompanyRepository
{
    private readonly CompanyDbFactory _companyDbFactory;

    public CompanyRepository(CompanyDbFactory companyDbFactory)
    {
        _companyDbFactory = companyDbFactory;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Users
            .Where(u => u.Status == UserStatus.Active)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Status == UserStatus.Active);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        context.Users.Update(user);
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return false;
        }

        // Soft delete
        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Users
            .Where(u => u.Department == department && u.Status == UserStatus.Active)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<bool> UserEmailExistsAsync(string email)
    {
        using var context = await _companyDbFactory.CreateDbContextAsync();
        
        return await context.Users
            .AnyAsync(u => u.Email == email);
    }
}