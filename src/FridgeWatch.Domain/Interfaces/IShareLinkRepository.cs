using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IShareLinkRepository : IRepository<ShareLink, int>
{
    Task<ShareLink?> GetByTokenAsync(string token);
    Task<PagedResult<ShareLink>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters);
}
