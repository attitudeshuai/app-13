using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class Household : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime? InviteCodeExpiresAt { get; set; }
    public int CreatedBy { get; set; }
    public int AutoArchiveDays { get; set; } = 7;

    public virtual ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();
    public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    public virtual ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
}
