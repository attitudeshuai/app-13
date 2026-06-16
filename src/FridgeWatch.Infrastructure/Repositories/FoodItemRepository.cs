using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class FoodItemRepository : Repository<FoodItem, int>, IFoodItemRepository
{
    public FoodItemRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<FoodItem>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet.Where(f => f.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(f =>
                f.Name.Contains(parameters.SearchTerm) ||
                f.Category.Contains(parameters.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderBy(f => f.ExpiryDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<FoodItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<List<FoodItem>> GetExpiringSoonAsync(int days)
    {
        var today = DateTime.UtcNow.Date;
        var expiryThreshold = today.AddDays(days);

        return await _dbSet
            .Where(f => f.ExpiryDate <= expiryThreshold &&
                        f.Status == FoodStatus.Fresh)
            .OrderBy(f => f.ExpiryDate)
            .ToListAsync();
    }

    public async Task<List<FoodItem>> GetExpiredAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _dbSet
            .Where(f => f.ExpiryDate < today &&
                        f.Status != FoodStatus.Consumed &&
                        f.Status != FoodStatus.Expired)
            .OrderBy(f => f.ExpiryDate)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, FoodStatus status)
    {
        var foodItem = await GetByIdAsync(id);
        if (foodItem != null)
        {
            foodItem.Status = status;
            foodItem.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(foodItem);
        }
    }

    protected override IQueryable<FoodItem> ApplySearch(IQueryable<FoodItem> query, string searchTerm)
    {
        return query.Where(f =>
            f.Name.Contains(searchTerm) ||
            f.Category.Contains(searchTerm));
    }

    protected override IQueryable<FoodItem> ApplySorting(IQueryable<FoodItem> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name),
            "expirydate" => descending ? query.OrderByDescending(f => f.ExpiryDate) : query.OrderBy(f => f.ExpiryDate),
            "purchasedate" => descending ? query.OrderByDescending(f => f.PurchaseDate) : query.OrderBy(f => f.PurchaseDate),
            "category" => descending ? query.OrderByDescending(f => f.Category) : query.OrderBy(f => f.Category),
            _ => query.OrderBy(f => f.ExpiryDate)
        };
    }
}
