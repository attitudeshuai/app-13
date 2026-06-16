using System.Linq.Expressions;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<List<TEntity>> GetAllAsync();
    Task<PagedResult<TEntity>> GetPagedAsync(QueryParameters parameters);
    Task<PagedResult<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, QueryParameters parameters);
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
}
