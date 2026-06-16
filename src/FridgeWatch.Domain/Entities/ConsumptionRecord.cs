using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class ConsumptionRecord : BaseEntity<int>
{
    public int FoodItemId { get; set; }
    public int UserId { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public virtual FoodItem? FoodItem { get; set; }
    public virtual User? User { get; set; }
}
