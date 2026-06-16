using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IHouseholdService
{
    Task<PagedResultDto<HouseholdDto>> GetListAsync(QueryParametersDto parameters, int? userId = null);
    Task<HouseholdDto> GetByIdAsync(int id);
    Task<HouseholdDto> CreateAsync(HouseholdCreateDto dto, int userId);
    Task<HouseholdDto> UpdateAsync(int id, HouseholdUpdateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task<HouseholdDto> ResetInviteCodeAsync(int householdId, ResetInviteCodeDto dto, int userId);
}
