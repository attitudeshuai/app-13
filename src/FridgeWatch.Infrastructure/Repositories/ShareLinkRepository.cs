using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class ShareLinkRepository : Repository<ShareLink, int>, IShareLinkRepository
{
    public ShareLinkRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<ShareLink?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(s => s.Household)
            .FirstOrDefaultAsync(s => s.Token == token && !s.IsRevoked);
    }

    public async Task<PagedResult<ShareLink>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet.Where(s => s.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(s => s.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ShareLink>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    protected override IQueryable<ShareLink> ApplySorting(IQueryable<ShareLink> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "createdat" => descending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            "expiresat" => descending ? query.OrderByDescending(s => s.ExpiresAt) : query.OrderBy(s => s.ExpiresAt),
            "viewcount" => descending ? query.OrderByDescending(s => s.ViewCount) : query.OrderBy(s => s.ViewCount),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };
    }
}
