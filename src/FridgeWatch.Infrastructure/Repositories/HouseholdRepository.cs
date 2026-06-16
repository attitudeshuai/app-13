using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class HouseholdRepository : Repository<Household, int>, IHouseholdRepository
{
    public HouseholdRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Household>> GetByUserIdAsync(int userId, QueryParameters parameters)
    {
        var query = _dbSet
            .Include(h => h.HouseholdMembers)
            .Where(h => h.HouseholdMembers.Any(hm => hm.UserId == userId));

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(h => h.Name.Contains(parameters.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(h => h.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Household>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Household?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _dbSet.FirstOrDefaultAsync(h => h.InviteCode == inviteCode);
    }

    protected override IQueryable<Household> ApplySearch(IQueryable<Household> query, string searchTerm)
    {
        return query.Where(h => h.Name.Contains(searchTerm));
    }
}
