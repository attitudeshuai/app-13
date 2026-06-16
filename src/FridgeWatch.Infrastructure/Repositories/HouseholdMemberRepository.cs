using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class HouseholdMemberRepository : Repository<HouseholdMember, int>, IHouseholdMemberRepository
{
    public HouseholdMemberRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<HouseholdMember>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(hm => hm.User)
            .Where(hm => hm.HouseholdId == householdId);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(hm => hm.User!.Username.Contains(parameters.SearchTerm));
        }

        query = query.OrderByDescending(hm => hm.JoinedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<HouseholdMember>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<HouseholdMember>> GetByUserIdAsync(int userId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(hm => hm.Household)
            .Where(hm => hm.UserId == userId);

        query = query.OrderByDescending(hm => hm.JoinedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<HouseholdMember>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<HouseholdMember?> GetByHouseholdAndUserAsync(int householdId, int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(hm => hm.HouseholdId == householdId && hm.UserId == userId);
    }

    public async Task<bool> IsHouseholdOwnerAsync(int householdId, int userId)
    {
        return await _dbSet.AnyAsync(hm =>
            hm.HouseholdId == householdId &&
            hm.UserId == userId &&
            hm.Role == HouseholdRole.Owner);
    }

    public async Task<bool> IsHouseholdMemberAsync(int householdId, int userId)
    {
        return await _dbSet.AnyAsync(hm =>
            hm.HouseholdId == householdId && hm.UserId == userId);
    }
}
