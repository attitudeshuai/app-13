using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Common;

public class FoodItemQueryParameters : QueryParameters
{
    public string? Category { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public FoodStatus? Status { get; set; }
}
