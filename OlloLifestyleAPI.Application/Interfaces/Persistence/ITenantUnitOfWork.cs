using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;

namespace OlloLifestyleAPI.Application.Interfaces.Persistence;

public interface ITenantUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}