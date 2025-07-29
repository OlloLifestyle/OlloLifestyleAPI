using Microsoft.EntityFrameworkCore.Storage;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Repositories.Master;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Companies = new GenericRepository<Company>(_context);
        Roles = new GenericRepository<Role>(_context);
        Permissions = new GenericRepository<Permission>(_context);
        UserRoles = new GenericRepository<UserRole>(_context);
        RolePermissions = new GenericRepository<RolePermission>(_context);
        UserCompanies = new GenericRepository<UserCompany>(_context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Company> Companies { get; }
    public IGenericRepository<Role> Roles { get; }
    public IGenericRepository<Permission> Permissions { get; }
    public IGenericRepository<UserRole> UserRoles { get; }
    public IGenericRepository<RolePermission> RolePermissions { get; }
    public IGenericRepository<UserCompany> UserCompanies { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}