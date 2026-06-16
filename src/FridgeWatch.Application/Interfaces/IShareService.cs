using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IShareService
{
    Task<ShareLinkDto> CreateShareLinkAsync(CreateShareLinkDto dto, int userId);
    Task<PagedResultDto<ShareLinkDto>> GetShareLinksByHouseholdIdAsync(int householdId, QueryParametersDto parameters, int userId);
    Task<ShareLinkDto> RevokeShareLinkAsync(int shareLinkId, int userId);
    Task<SharedFoodItemsDto> GetSharedFoodItemsAsync(string token);
}
