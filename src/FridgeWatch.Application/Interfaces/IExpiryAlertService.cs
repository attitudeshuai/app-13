using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IExpiryAlertService
{
    Task<PagedResultDto<ExpiryAlertDto>> GetListAsync(QueryParametersDto parameters, int? foodItemId = null);
    Task<PagedResultDto<ExpiryAlertDto>> GetMineAsync(int userId, QueryParametersDto parameters, bool? isRead = null);
    Task<ExpiryAlertDto> GetByIdAsync(int id);
    Task<ExpiryAlertDto> CreateAsync(ExpiryAlertCreateDto dto, int userId);
    Task<ExpiryAlertDto> UpdateAsync(int id, ExpiryAlertUpdateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task BatchDeleteAsync(IEnumerable<int> ids, int userId);
}
