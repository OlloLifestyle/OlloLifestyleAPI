using OlloLifestyleAPI.Core.Entities.Master;

namespace OlloLifestyleAPI.Application.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<Permission> Permissions { get; }
    IGenericRepository<UserRole> UserRoles { get; }
    IGenericRepository<RolePermission> RolePermissions { get; }
    IGenericRepository<UserCompany> UserCompanies { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}