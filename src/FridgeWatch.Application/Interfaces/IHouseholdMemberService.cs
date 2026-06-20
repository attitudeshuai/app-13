using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IHouseholdMemberService
{
    Task<PagedResultDto<HouseholdMemberDto>> GetListAsync(QueryParametersDto parameters, int? householdId = null);
    Task<PagedResultDto<HouseholdMemberDto>> GetMineAsync(int userId, QueryParametersDto parameters);
    Task<HouseholdMemberDto> GetByIdAsync(int id);
    Task<HouseholdMemberDto> JoinHouseholdAsync(string inviteCode, int userId);
    Task<HouseholdMemberDto> UpdateAsync(int id, HouseholdMemberUpdateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task LeaveHouseholdAsync(int householdId, int userId);
}
