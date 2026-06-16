using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Entities;

public class ShoppingListItem : BaseEntity<int>
{
    public int ShoppingListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public StorageLocation? StorageLocation { get; set; }
    public int? ExpiryDays { get; set; }
    public string? Notes { get; set; }
    public bool IsPurchased { get; set; }

    public virtual ShoppingList? ShoppingList { get; set; }
}
