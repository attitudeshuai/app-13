using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class ExpiryAlertDto
{
    public int Id { get; set; }
    public int FoodItemId { get; set; }
    public int UserId { get; set; }
    public AlertType AlertType { get; set; }
    public DateTime AlertDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public FoodItemDto? FoodItem { get; set; }
}

public class ExpiryAlertCreateDto
{
    public int FoodItemId { get; set; }
    public AlertType AlertType { get; set; }
    public DateTime AlertDate { get; set; }
}

public class ExpiryAlertUpdateDto
{
    public bool? IsRead { get; set; }
    public AlertType? AlertType { get; set; }
}
