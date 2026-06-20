using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class User : BaseEntity<int>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int? DefaultHouseholdId { get; set; }

    public virtual Household? DefaultHousehold { get; set; }
    public virtual ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();
    public virtual ICollection<ExpiryAlert> ExpiryAlerts { get; set; } = new List<ExpiryAlert>();
    public virtual ICollection<ConsumptionRecord> ConsumptionRecords { get; set; } = new List<ConsumptionRecord>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
