namespace FridgeWatch.Application.DTOs;

public class ShareLinkDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string HouseholdName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public int ViewCount { get; set; }
    public bool IsExpired { get; set; }
}

public class CreateShareLinkDto
{
    public int HouseholdId { get; set; }
    public int ValidDays { get; set; } = 7;
}

public class SharedFoodItemsDto
{
    public string HouseholdName { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int FoodItemCount { get; set; }
    public List<FoodItemDto> FoodItems { get; set; } = new();
}
