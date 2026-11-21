using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Data;

namespace Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SchoolDbContext _schoolDbContext;
    private readonly Dictionary<string, object> _repositories = [];
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(SchoolDbContext context)
        => _schoolDbContext = context;

    public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
    {
        var key = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(key))
        {
            _repositories[key] = new GenericRepository<TEntity, TKey>(_schoolDbContext);
        }

        return (IGenericRepository<TEntity, TKey>)_repositories[key];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _schoolDbContext.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _schoolDbContext.Database.BeginTransactionAsync();
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
}
