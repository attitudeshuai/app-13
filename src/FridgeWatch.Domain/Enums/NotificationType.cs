namespace FridgeWatch.Domain.Enums;

public enum NotificationType
{
    ExpiryAlert = 1,
    HouseholdActivity = 2,
    System = 3
}

public enum NotificationCategory
{
    NearExpiry = 1,
    Expired = 2,
    CustomAlert = 3,
    MemberJoined = 4,
    MemberLeft = 5,
    FoodItemAdded = 6,
    FoodItemConsumed = 7,
    FoodItemExpired = 8,
    ShoppingListUpdated = 9,
    SystemAnnouncement = 10,
    WelcomeMessage = 11,
    SecurityAlert = 12
}
