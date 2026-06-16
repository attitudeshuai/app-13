using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Common;

public static class FoodStatusHelper
{
    public static FoodStatus CalculateStatus(DateTime expiryDate, decimal quantity)
    {
        if (quantity <= 0)
        {
            return FoodStatus.Consumed;
        }

        return CalculateStatus(expiryDate);
    }

    public static FoodStatus CalculateStatus(DateTime expiryDate)
    {
        var today = DateTime.UtcNow.Date;
        var daysToExpiry = (expiryDate.Date - today).Days;

        if (daysToExpiry < 0)
        {
            return FoodStatus.Expired;
        }
        else if (daysToExpiry <= 3)
        {
            return FoodStatus.NearExpiry;
        }
        else
        {
            return FoodStatus.Fresh;
        }
    }
}
