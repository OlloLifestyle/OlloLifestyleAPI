using Microsoft.EntityFrameworkCore.Storage;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Core.Entities.Tenant;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Repositories.Tenant;

public class TenantUnitOfWork : ITenantUnitOfWork
{
    private readonly CompanyDbContext _context;
    private IDbContextTransaction? _transaction;

    public TenantUnitOfWork(CompanyDbContext context)
    {
        _context = context;
        Employees = new TenantGenericRepository<Employee>(_context);
        Products = new TenantGenericRepository<Product>(_context);
        Orders = new TenantGenericRepository<Order>(_context);
        OrderItems = new TenantGenericRepository<OrderItem>(_context);
    }

    public IGenericRepository<Employee> Employees { get; }
    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }

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