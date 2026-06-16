using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class ShoppingListDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ItemCount { get; set; }
    public int PurchasedCount { get; set; }
    public List<ShoppingListItemDto> Items { get; set; } = new();
}

public class ShoppingListItemDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public StorageLocation? StorageLocation { get; set; }
    public int? ExpiryDays { get; set; }
    public string? Notes { get; set; }
    public bool IsPurchased { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShoppingListCreateDto
{
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ShoppingListItemCreateDto> Items { get; set; } = new();
}

public class ShoppingListUpdateDto
{
    public string? Name { get; set; }
    public List<ShoppingListItemCreateDto>? Items { get; set; }
}

public class ShoppingListItemCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public StorageLocation? StorageLocation { get; set; }
    public int? ExpiryDays { get; set; }
    public string? Notes { get; set; }
}

public class ShoppingListItemUpdateDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public int? ExpiryDays { get; set; }
    public string? Notes { get; set; }
    public bool? IsPurchased { get; set; }
}

public class ShoppingListConvertDto
{
    public int ShoppingListId { get; set; }
    public bool PurchaseAll { get; set; } = true;
    public List<ShoppingListItemConvertDto>? ItemOverrides { get; set; }
}

public class ShoppingListItemConvertDto
{
    public int ItemId { get; set; }
    public StorageLocation StorageLocation { get; set; }
    public int ExpiryDays { get; set; } = 7;
    public string? PhotoUrl { get; set; }
}
