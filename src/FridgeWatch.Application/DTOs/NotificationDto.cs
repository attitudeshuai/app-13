using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class NotificationDto
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Data { get; set; }
    public int? HouseholdId { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationCreateDto
{
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Data { get; set; }
    public int? HouseholdId { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class NotificationBatchCreateDto
{
    public List<int> UserIds { get; set; } = new();
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Data { get; set; }
    public int? HouseholdId { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class NotificationUpdateDto
{
    public bool? IsRead { get; set; }
}

public class NotificationBatchMarkReadDto
{
    public List<long> Ids { get; set; } = new();
}

public class NotificationBatchDeleteDto
{
    public List<long> Ids { get; set; } = new();
}

public class NotificationUnreadCountDto
{
    public int Total { get; set; }
    public int ExpiryAlert { get; set; }
    public int HouseholdActivity { get; set; }
    public int System { get; set; }
}

public class NotificationQueryParametersDto
{
    public NotificationType? Type { get; set; }
    public NotificationCategory? Category { get; set; }
    public bool? IsRead { get; set; }
    public int? HouseholdId { get; set; }
}
