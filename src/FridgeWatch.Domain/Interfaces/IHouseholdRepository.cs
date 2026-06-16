using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IHouseholdRepository : IRepository<Household, int>
{
    Task<PagedResult<Household>> GetByUserIdAsync(int userId, QueryParameters parameters);
    Task<Household?> GetByInviteCodeAsync(string inviteCode);
}
