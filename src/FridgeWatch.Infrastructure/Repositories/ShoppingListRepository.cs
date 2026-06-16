using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class ShoppingListRepository : Repository<ShoppingList, int>, IShoppingListRepository
{
    public ShoppingListRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<ShoppingList>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet.Where(sl => sl.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(sl => sl.Name.Contains(parameters.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(sl => sl.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ShoppingList>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<ShoppingList?> GetWithItemsByIdAsync(int id)
    {
        return await _dbSet
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    protected override IQueryable<ShoppingList> ApplySearch(IQueryable<ShoppingList> query, string searchTerm)
    {
        return query.Where(sl => sl.Name.Contains(searchTerm));
    }

    protected override IQueryable<ShoppingList> ApplySorting(IQueryable<ShoppingList> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(sl => sl.Name) : query.OrderBy(sl => sl.Name),
            "createdat" => descending ? query.OrderByDescending(sl => sl.CreatedAt) : query.OrderBy(sl => sl.CreatedAt),
            _ => query.OrderByDescending(sl => sl.CreatedAt)
        };
    }
}
