namespace FridgeWatch.Application.DTOs;

public class ConsumptionRecordDto
{
    public int Id { get; set; }
    public int FoodItemId { get; set; }
    public int UserId { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public DateTime ConsumedAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public FoodItemDto? FoodItem { get; set; }
    public UserDto? User { get; set; }
}

public class ConsumptionRecordCreateDto
{
    public int FoodItemId { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? Note { get; set; }
}

public class ConsumptionRecordUpdateDto
{
    public decimal? ConsumedQuantity { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? Note { get; set; }
}
