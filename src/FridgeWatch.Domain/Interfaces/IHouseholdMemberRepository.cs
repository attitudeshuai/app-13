using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IHouseholdMemberRepository : IRepository<HouseholdMember, int>
{
    Task<PagedResult<HouseholdMember>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters);
    Task<PagedResult<HouseholdMember>> GetByUserIdAsync(int userId, QueryParameters parameters);
    Task<HouseholdMember?> GetByHouseholdAndUserAsync(int householdId, int userId);
    Task<bool> IsHouseholdOwnerAsync(int householdId, int userId);
    Task<bool> IsHouseholdMemberAsync(int householdId, int userId);
}
