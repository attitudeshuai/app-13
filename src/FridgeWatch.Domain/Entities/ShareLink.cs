using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class ShareLink : BaseEntity<int>
{
    public int HouseholdId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public int ViewCount { get; set; }

    public virtual Household? Household { get; set; }
}
