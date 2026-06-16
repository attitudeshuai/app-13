using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Entities;

public class FoodItem : BaseEntity<int>
{
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public StorageLocation StorageLocation { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public FoodStatus Status { get; set; } = FoodStatus.Fresh;

    public virtual Household? Household { get; set; }
    public virtual ICollection<ExpiryAlert> ExpiryAlerts { get; set; } = new List<ExpiryAlert>();
    public virtual ICollection<ConsumptionRecord> ConsumptionRecords { get; set; } = new List<ConsumptionRecord>();
}
