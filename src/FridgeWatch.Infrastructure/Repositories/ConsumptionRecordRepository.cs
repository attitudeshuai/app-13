using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class ConsumptionRecordRepository : Repository<ConsumptionRecord, int>, IConsumptionRecordRepository
{
    public ConsumptionRecordRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<ConsumptionRecord>> GetByUserIdAsync(int userId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(cr => cr.FoodItem)
            .Where(cr => cr.UserId == userId);

        query = query.OrderByDescending(cr => cr.ConsumedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ConsumptionRecord>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<ConsumptionRecord>> GetByFoodItemIdAsync(int foodItemId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(cr => cr.User)
            .Where(cr => cr.FoodItemId == foodItemId);

        query = query.OrderByDescending(cr => cr.ConsumedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ConsumptionRecord>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<ConsumptionRecord>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(cr => cr.FoodItem)
            .Include(cr => cr.User)
            .Where(cr => cr.FoodItem!.HouseholdId == householdId);

        query = query.OrderByDescending(cr => cr.ConsumedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ConsumptionRecord>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<decimal> GetTotalConsumedQuantityByFoodItemAsync(int foodItemId)
    {
        return await _dbSet
            .Where(cr => cr.FoodItemId == foodItemId)
            .SumAsync(cr => cr.ConsumedQuantity);
    }
}
