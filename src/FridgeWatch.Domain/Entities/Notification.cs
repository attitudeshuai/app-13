using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Entities;

public class Notification : BaseEntity<long>
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
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual User? User { get; set; }
}
