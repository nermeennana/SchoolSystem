using System;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Persistence.Data;

namespace Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SchoolDbContext _schoolDbContext;
    private readonly Dictionary<string, object> _repositories = new();
    
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
}
