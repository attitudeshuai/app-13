using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class FoodItemQueryParametersDto : QueryParametersDto
{
    public string? Category { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public FoodStatus? Status { get; set; }
    public bool IncludeArchived { get; set; } = false;
}
