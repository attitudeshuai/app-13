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

    public static int GetDaysExpired(DateTime expiryDate)
    {
        var today = DateTime.UtcNow.Date;
        return (today - expiryDate.Date).Days;
    }

    public static bool ShouldBeArchived(DateTime expiryDate, FoodStatus currentStatus, int autoArchiveDays)
    {
        if (currentStatus == FoodStatus.Archived || currentStatus == FoodStatus.Consumed)
        {
            return false;
        }

        var daysExpired = GetDaysExpired(expiryDate);
        return daysExpired >= autoArchiveDays;
    }
}
