namespace FridgeWatch.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public int? HouseholdId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTime OperatedAt { get; set; } = DateTime.UtcNow;
}
