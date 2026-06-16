using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class ShoppingList : BaseEntity<int>
{
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual Household? Household { get; set; }
    public virtual ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
