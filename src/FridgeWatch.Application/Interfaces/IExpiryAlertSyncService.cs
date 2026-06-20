using FridgeWatch.Domain.Entities;

namespace FridgeWatch.Application.Interfaces;

public interface IExpiryAlertSyncService
{
    Task SyncAlertsForFoodItemAsync(FoodItem foodItem);
    Task<ExpiryAlertScanResult> ScanAndSyncAllAlertsAsync();
}

public class ExpiryAlertScanResult
{
    public int TotalFoodItemsProcessed { get; set; }
    public int FoodItemsStatusUpdated { get; set; }
    public int AlertsCreated { get; set; }
    public int AlertsRemoved { get; set; }
    public int NearExpiryAlertsCreated { get; set; }
    public int ExpiredAlertsCreated { get; set; }
    public DateTime ScanStartTime { get; set; }
    public DateTime ScanEndTime { get; set; }
}
