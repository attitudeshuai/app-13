using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class FoodItemDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public StorageLocation StorageLocation { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public FoodStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysToExpiry { get; set; }
}

public class FoodItemDetailDto : FoodItemDto
{
    public string? OriginalPhotoUrl { get; set; }
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
}

public class FoodItemStatusUpdateDto
{
    public FoodStatus Status { get; set; }
}

public class FoodItemImportRowDto
{
    public int RowNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string StorageLocation { get; set; } = string.Empty;
    public string PurchaseDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public class FoodItemImportResultDto
{
    public int TotalRows { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<FoodItemImportErrorDto> Errors { get; set; } = new();
    public List<FoodItemDto> ImportedItems { get; set; } = new();
}

public class FoodItemImportErrorDto
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
