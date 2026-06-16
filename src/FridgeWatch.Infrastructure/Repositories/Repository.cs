using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Interfaces;
using System.Linq.Expressions;

namespace FridgeWatch.Infrastructure.Repositories;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(QueryParameters parameters)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = ApplySearch(query, parameters.SearchTerm);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public virtual async Task<PagedResult<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        QueryParameters parameters)
    {
        var query = _dbSet.Where(predicate);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = ApplySearch(query, parameters.SearchTerm);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string searchTerm)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sortBy, bool descending)
    {
        return query;
    }
}
