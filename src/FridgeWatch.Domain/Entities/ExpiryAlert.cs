using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Entities;

public class ExpiryAlert : BaseEntity<int>
{
    public int FoodItemId { get; set; }
    public int UserId { get; set; }
    public AlertType AlertType { get; set; }
    public DateTime AlertDate { get; set; }
    public bool IsRead { get; set; } = false;

    public virtual FoodItem? FoodItem { get; set; }
    public virtual User? User { get; set; }
}
