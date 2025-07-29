using Microsoft.EntityFrameworkCore.Storage;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.Master;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Repositories.Master;

public class MasterUnitOfWork : IMasterUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public MasterUnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new MasterGenericRepository<User>(_context);
        Companies = new MasterGenericRepository<Company>(_context);
        Roles = new MasterGenericRepository<Role>(_context);
        Permissions = new MasterGenericRepository<Permission>(_context);
        UserRoles = new MasterGenericRepository<UserRole>(_context);
        RolePermissions = new MasterGenericRepository<RolePermission>(_context);
        UserCompanies = new MasterGenericRepository<UserCompany>(_context);
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