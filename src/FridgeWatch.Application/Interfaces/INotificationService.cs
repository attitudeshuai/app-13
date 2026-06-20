using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResultDto<NotificationDto>> GetMineAsync(
        int userId,
        QueryParametersDto parameters,
        NotificationQueryParametersDto? filter = null);

    Task<NotificationDto> GetByIdAsync(long id, int userId);

    Task<NotificationDto> CreateAsync(NotificationCreateDto dto);

    Task<List<NotificationDto>> BatchCreateAsync(NotificationBatchCreateDto dto);

    Task<NotificationDto> UpdateAsync(long id, NotificationUpdateDto dto, int userId);

    Task DeleteAsync(long id, int userId);

    Task BatchDeleteAsync(IEnumerable<long> ids, int userId);

    Task<NotificationUnreadCountDto> GetUnreadCountAsync(int userId);

    Task<int> GetTotalUnreadCountAsync(int userId);

    Task MarkAsReadAsync(long id, int userId);

    Task MarkAllAsReadAsync(int userId, NotificationType? type = null);

    Task BatchMarkAsReadAsync(IEnumerable<long> ids, int userId);

    Task SyncExpiryAlertsToNotificationsAsync(int userId);

    Task GenerateHouseholdActivityNotificationAsync(
        int householdId,
        NotificationCategory category,
        string title,
        string content,
        int? operatorId = null,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        string? data = null);
}
