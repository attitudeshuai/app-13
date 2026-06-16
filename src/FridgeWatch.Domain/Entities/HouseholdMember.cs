using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Entities;

public class HouseholdMember : BaseEntity<int>
{
    public int HouseholdId { get; set; }
    public int UserId { get; set; }
    public HouseholdRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual Household? Household { get; set; }
    public virtual User? User { get; set; }
}
