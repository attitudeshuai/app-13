using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<NotificationDto>> GetMineAsync(
        int userId,
        QueryParametersDto parameters,
        NotificationQueryParametersDto? filter = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        PagedResult<Notification> result;
        if (filter != null)
        {
            result = await _unitOfWork.Notifications.GetByUserIdAsync(
                userId,
                queryParams,
                filter.Type,
                filter.Category,
                filter.IsRead,
                filter.HouseholdId);
        }
        else
        {
            result = await _unitOfWork.Notifications.GetByUserIdAsync(userId, queryParams);
        }

        return _mapper.Map<PagedResultDto<NotificationDto>>(result);
    }

    public async Task<NotificationDto> GetByIdAsync(long id, int userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null)
        {
            throw new BusinessException("通知不存在");
        }

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("无权访问该通知");
        }

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task<NotificationDto> CreateAsync(NotificationCreateDto dto)
    {
        var notification = _mapper.Map<Notification>(dto);
        notification.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task<List<NotificationDto>> BatchCreateAsync(NotificationBatchCreateDto dto)
    {
        if (dto.UserIds == null || !dto.UserIds.Any())
        {
            throw new BusinessException("请指定接收通知的用户");
        }

        var notifications = new List<Notification>();
        foreach (var userId in dto.UserIds.Distinct())
        {
            notifications.Add(new Notification
            {
                UserId = userId,
                Type = dto.Type,
                Category = dto.Category,
                Title = dto.Title,
                Content = dto.Content,
                Data = dto.Data,
                HouseholdId = dto.HouseholdId,
                RelatedEntityId = dto.RelatedEntityId,
                RelatedEntityType = dto.RelatedEntityType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        var createdNotifications = new List<Notification>();
        foreach (var notification in notifications)
        {
            createdNotifications.Add(await _unitOfWork.Notifications.AddAsync(notification));
        }
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<NotificationDto>>(createdNotifications);
    }

    public async Task<NotificationDto> UpdateAsync(long id, NotificationUpdateDto dto, int userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null)
        {
            throw new BusinessException("通知不存在");
        }

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能修改自己的通知");
        }

        _mapper.Map(dto, notification);
        notification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Notifications.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task DeleteAsync(long id, int userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null)
        {
            throw new BusinessException("通知不存在");
        }

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能删除自己的通知");
        }

        await _unitOfWork.Notifications.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task BatchDeleteAsync(IEnumerable<long> ids, int userId)
    {
        var idList = ids.ToList();
        if (!idList.Any())
        {
            throw new BusinessException("请选择要删除的通知");
        }

        var notifications = await _unitOfWork.Notifications.FindAsync(
            n => idList.Contains(n.Id) && n.UserId == userId);
        var ownedIds = notifications.Select(n => n.Id).ToList();

        if (!ownedIds.Any())
        {
            throw new BusinessException("没有可删除的通知");
        }

        await _unitOfWork.Notifications.DeleteByIdsAsync(ownedIds);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<NotificationUnreadCountDto> GetUnreadCountAsync(int userId)
    {
        var count = await _unitOfWork.Notifications.GetUnreadCountByTypeAsync(userId);
        return _mapper.Map<NotificationUnreadCountDto>(count);
    }

    public async Task<int> GetTotalUnreadCountAsync(int userId)
    {
        return await _unitOfWork.Notifications.GetTotalUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(long id, int userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null)
        {
            throw new BusinessException("通知不存在");
        }

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("只能标记自己的通知为已读");
        }

        await _unitOfWork.Notifications.MarkAsReadAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId, NotificationType? type = null)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId, type);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task BatchMarkAsReadAsync(IEnumerable<long> ids, int userId)
    {
        var idList = ids.ToList();
        if (!idList.Any())
        {
            throw new BusinessException("请选择要标记的通知");
        }

        await _unitOfWork.Notifications.BatchMarkAsReadAsync(idList, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SyncExpiryAlertsToNotificationsAsync(int userId)
    {
        var alerts = await _unitOfWork.ExpiryAlerts.FindAsync(ea => ea.UserId == userId);
        var existingAlertIds = await _unitOfWork.Notifications.FindAsync(
            n => n.UserId == userId && n.Type == NotificationType.ExpiryAlert && n.RelatedEntityType == "ExpiryAlert");
        var existingIds = existingAlertIds.Select(n => n.RelatedEntityId).ToHashSet();

        var newNotifications = new List<Notification>();
        foreach (var alert in alerts)
        {
            if (existingIds.Contains(alert.Id))
            {
                continue;
            }

            NotificationCategory category;
            string title;
            string content;

            switch (alert.AlertType)
            {
                case AlertType.NearExpiry:
                    category = NotificationCategory.NearExpiry;
                    title = "食材临期提醒";
                    content = alert.FoodItem != null
                        ? $"您的食材「{alert.FoodItem.Name}」将在 {(alert.AlertDate - DateTime.UtcNow).Days} 天后过期，请尽快食用"
                        : "您有食材即将过期";
                    break;
                case AlertType.Expired:
                    category = NotificationCategory.Expired;
                    title = "食材过期提醒";
                    content = alert.FoodItem != null
                        ? $"您的食材「{alert.FoodItem.Name}」已过期，请及时处理"
                        : "您有食材已过期";
                    break;
                default:
                    category = NotificationCategory.CustomAlert;
                    title = "自定义提醒";
                    content = alert.FoodItem != null
                        ? $"关于食材「{alert.FoodItem.Name}」的提醒"
                        : "您有一条自定义提醒";
                    break;
            }

            newNotifications.Add(new Notification
            {
                UserId = userId,
                Type = NotificationType.ExpiryAlert,
                Category = category,
                Title = title,
                Content = content,
                HouseholdId = alert.FoodItem?.HouseholdId,
                RelatedEntityId = alert.Id,
                RelatedEntityType = "ExpiryAlert",
                IsRead = alert.IsRead,
                ReadAt = alert.IsRead ? DateTime.UtcNow : null,
                CreatedAt = alert.CreatedAt
            });
        }

        foreach (var notification in newNotifications)
        {
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task GenerateHouseholdActivityNotificationAsync(
        int householdId,
        NotificationCategory category,
        string title,
        string content,
        int? operatorId = null,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        string? data = null)
    {
        var members = await _unitOfWork.HouseholdMembers.FindAsync(hm => hm.HouseholdId == householdId);
        var userIds = members
            .Select(hm => hm.UserId)
            .Where(id => !operatorId.HasValue || id != operatorId.Value)
            .Distinct()
            .ToList();

        if (!userIds.Any())
        {
            return;
        }

        foreach (var userId in userIds)
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = userId,
                Type = NotificationType.HouseholdActivity,
                Category = category,
                Title = title,
                Content = content,
                Data = data,
                HouseholdId = householdId,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
