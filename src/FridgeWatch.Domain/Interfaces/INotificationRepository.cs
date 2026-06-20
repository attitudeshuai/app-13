using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification, long>
{
    Task<PagedResult<Notification>> GetByUserIdAsync(
        int userId,
        QueryParameters parameters,
        NotificationType? type = null,
        NotificationCategory? category = null,
        bool? isRead = null,
        int? householdId = null);

    Task<NotificationTypeUnreadCount> GetUnreadCountByTypeAsync(int userId);

    Task<int> GetTotalUnreadCountAsync(int userId);

    Task MarkAsReadAsync(long id);

    Task MarkAllAsReadAsync(int userId, NotificationType? type = null);

    Task BatchMarkAsReadAsync(IEnumerable<long> ids, int userId);

    Task DeleteByIdsAsync(IEnumerable<long> ids);
}

public class NotificationTypeUnreadCount
{
    public int Total { get; set; }
    public int ExpiryAlert { get; set; }
    public int HouseholdActivity { get; set; }
    public int System { get; set; }
}
