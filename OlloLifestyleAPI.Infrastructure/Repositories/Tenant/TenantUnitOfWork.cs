using Microsoft.EntityFrameworkCore.Storage;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Repositories.Tenant;

public class TenantUnitOfWork : ITenantUnitOfWork
{
    private readonly CompanyDbContext _context;
    private IDbContextTransaction? _transaction;

    public TenantUnitOfWork(CompanyDbContext context)
    {
        _context = context;
        Users = new TenantGenericRepository<User>(_context);
    }

    public IGenericRepository<User> Users { get; }

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