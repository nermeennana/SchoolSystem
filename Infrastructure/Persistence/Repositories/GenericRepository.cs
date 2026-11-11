using System;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    private readonly SchoolDbContext _schoolDbContext;

    public GenericRepository(SchoolDbContext context)
    => _schoolDbContext = context;

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    => await _schoolDbContext.Set<TEntity>().ToListAsync();

    public async Task<TEntity?> GetByIdAsync(TKey id)
    => await _schoolDbContext.Set<TEntity>().FindAsync(id);

    public void Add(TEntity entity)
    => _schoolDbContext.Set<TEntity>().Add(entity);

    public void Update(TEntity entity)
    => _schoolDbContext.Set<TEntity>().Update(entity);
    
    public void Delete(TEntity entity)
    => _schoolDbContext.Set<TEntity>().Remove(entity);
}
