using System;
using DomainLayer.Models;

namespace DomainLayer.Contracts;

public interface IUnitOfWork
{
    IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>;
    Task<int> SaveChangesAsync();
}
