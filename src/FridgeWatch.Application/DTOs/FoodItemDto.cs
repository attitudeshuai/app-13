using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class FoodItemDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public StorageLocation StorageLocation { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public FoodStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysToExpiry { get; set; }
}

public class FoodItemCreateDto
{
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public StorageLocation StorageLocation { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}

public class FoodItemUpdateDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? PhotoUrl { get; set; }
}

public class FoodItemStatusUpdateDto
{
    public FoodStatus Status { get; set; }
}
