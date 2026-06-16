using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class ExpiryAlertRepository : Repository<ExpiryAlert, int>, IExpiryAlertRepository
{
    public ExpiryAlertRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<ExpiryAlert>> GetByUserIdAsync(int userId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(ea => ea.FoodItem)
            .Where(ea => ea.UserId == userId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(ea => ea.FoodItem!.Name.Contains(parameters.SearchTerm));
        }

        query = query.OrderByDescending(ea => ea.AlertDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ExpiryAlert>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<ExpiryAlert>> GetByFoodItemIdAsync(int foodItemId, QueryParameters parameters)
    {
        var query = _dbSet.Where(ea => ea.FoodItemId == foodItemId);
        query = query.OrderByDescending(ea => ea.AlertDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ExpiryAlert>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _dbSet.CountAsync(ea => ea.UserId == userId && !ea.IsRead);
    }

    public async Task MarkAsReadAsync(int id)
    {
        var alert = await GetByIdAsync(id);
        if (alert != null)
        {
            alert.IsRead = true;
            alert.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(alert);
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var alerts = await _dbSet.Where(ea => ea.UserId == userId && !ea.IsRead).ToListAsync();
        foreach (var alert in alerts)
        {
            alert.IsRead = true;
            alert.UpdatedAt = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(alerts);
    }
}
