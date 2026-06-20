using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification, long>, INotificationRepository
{
    public NotificationRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Notification>> GetByUserIdAsync(
        int userId,
        QueryParameters parameters,
        NotificationType? type = null,
        NotificationCategory? category = null,
        bool? isRead = null,
        int? householdId = null)
    {
        var query = _dbSet.Where(n => n.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(n => n.Category == category.Value);
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (householdId.HasValue)
        {
            query = query.Where(n => n.HouseholdId == householdId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(n =>
                n.Title.Contains(parameters.SearchTerm) ||
                n.Content.Contains(parameters.SearchTerm));
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Notification>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<NotificationTypeUnreadCount> GetUnreadCountByTypeAsync(int userId)
    {
        var unread = _dbSet.Where(n => n.UserId == userId && !n.IsRead);

        return new NotificationTypeUnreadCount
        {
            Total = await unread.CountAsync(),
            ExpiryAlert = await unread.CountAsync(n => n.Type == NotificationType.ExpiryAlert),
            HouseholdActivity = await unread.CountAsync(n => n.Type == NotificationType.HouseholdActivity),
            System = await unread.CountAsync(n => n.Type == NotificationType.System)
        };
    }

    public async Task<int> GetTotalUnreadCountAsync(int userId)
    {
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(long id)
    {
        var notification = await GetByIdAsync(id);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(notification);
        }
    }

    public async Task MarkAllAsReadAsync(int userId, NotificationType? type = null)
    {
        var query = _dbSet.Where(n => n.UserId == userId && !n.IsRead);

        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        var notifications = await query.ToListAsync();
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(notifications);
    }

    public async Task BatchMarkAsReadAsync(IEnumerable<long> ids, int userId)
    {
        var idList = ids.ToList();
        if (!idList.Any())
        {
            return;
        }

        var notifications = await _dbSet
            .Where(n => idList.Contains(n.Id) && n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(notifications);
    }

    public async Task DeleteByIdsAsync(IEnumerable<long> ids)
    {
        var notifications = await _dbSet.Where(n => ids.Contains(n.Id)).ToListAsync();
        _dbSet.RemoveRange(notifications);
    }
}
