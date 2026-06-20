namespace FridgeWatch.Application.DTOs;

public enum TimeRangeType
{
    Custom,
    Last7Days,
    Last30Days
}

public class StatsOverviewQueryDto
{
    public int? HouseholdId { get; set; }
    public TimeRangeType TimeRange { get; set; } = TimeRangeType.Last7Days;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class StatsOverviewDto
{
    public int TotalHouseholds { get; set; }
    public int TotalFoodItems { get; set; }
    public int FreshCount { get; set; }
    public int NearExpiryCount { get; set; }
    public int ExpiredCount { get; set; }
    public int ConsumedCount { get; set; }
    public int UnreadAlerts { get; set; }
    public decimal TotalConsumedQuantity { get; set; }
    public List<FoodCategoryStatDto> CategoryStats { get; set; } = new();
    public List<FoodLocationStatDto> LocationStats { get; set; } = new();
}

public class FoodCategoryStatDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class FoodLocationStatDto
{
    public string Location { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TrendItemDto
{
    public DateTime Date { get; set; }
    public int Value { get; set; }
}

public class StatsTrendDto
{
    public List<TrendItemDto> FoodAddedTrend { get; set; } = new();
    public List<TrendItemDto> ConsumptionTrend { get; set; } = new();
    public List<TrendItemDto> ExpiryTrend { get; set; } = new();
}

public class StatsTrendQueryDto : QueryParametersDto
{
    public TimeRangeType TimeRange { get; set; } = TimeRangeType.Last7Days;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? HouseholdId { get; set; }
}

public class MemberActivityQueryDto
{
    public int HouseholdId { get; set; }
    public TimeRangeType TimeRange { get; set; } = TimeRangeType.Last7Days;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class MemberActivityStatsDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int AddedFoodCount { get; set; }
    public int ConsumptionCount { get; set; }
    public int HandledAlertsCount { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public int TotalActivities => AddedFoodCount + ConsumptionCount + HandledAlertsCount;
}
