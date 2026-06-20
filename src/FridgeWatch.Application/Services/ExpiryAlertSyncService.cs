using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using Microsoft.Extensions.Logging;

namespace FridgeWatch.Application.Services;

public class ExpiryAlertSyncService : IExpiryAlertSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpiryAlertSyncService> _logger;

    public ExpiryAlertSyncService(IUnitOfWork unitOfWork, ILogger<ExpiryAlertSyncService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SyncAlertsForFoodItemAsync(FoodItem foodItem)
    {
        var (_, nearExpiryAlertsCreated, expiredAlertsCreated, alertsRemoved, _) = await ProcessFoodItemAsync(foodItem);
        _logger.LogInformation("食材 {FoodItemId}({FoodItemName}) 提醒同步完成：新增临期{NearExpiry}个，新增过期{Expired}个，移除{Removed}个",
            foodItem.Id, foodItem.Name, nearExpiryAlertsCreated, expiredAlertsCreated, alertsRemoved);
    }

    public async Task<ExpiryAlertScanResult> ScanAndSyncAllAlertsAsync()
    {
        var result = new ExpiryAlertScanResult
        {
            ScanStartTime = DateTime.UtcNow
        };

        _logger.LogInformation("开始全量扫描食材提醒...");

        try
        {
            var allFoodItems = await _unitOfWork.FoodItems.GetAllAsync();

            foreach (var foodItem in allFoodItems)
            {
                result.TotalFoodItemsProcessed++;

                var (statusUpdated, nearExpiryCreated, expiredCreated, alertsRemoved, _) = await ProcessFoodItemAsync(foodItem);

                if (statusUpdated)
                {
                    result.FoodItemsStatusUpdated++;
                }

                result.NearExpiryAlertsCreated += nearExpiryCreated;
                result.ExpiredAlertsCreated += expiredCreated;
                result.AlertsCreated += nearExpiryCreated + expiredCreated;
                result.AlertsRemoved += alertsRemoved;
            }

            result.ScanEndTime = DateTime.UtcNow;

            _logger.LogInformation(
                "全量扫描完成：处理食材{Total}个，更新状态{StatusUpdated}个，新增临期提醒{NearExpiry}个，新增过期提醒{Expired}个，移除提醒{Removed}个，耗时{Elapsed}秒",
                result.TotalFoodItemsProcessed,
                result.FoodItemsStatusUpdated,
                result.NearExpiryAlertsCreated,
                result.ExpiredAlertsCreated,
                result.AlertsRemoved,
                (result.ScanEndTime - result.ScanStartTime).TotalSeconds.ToString("F2"));
        }
        catch (Exception ex)
        {
            result.ScanEndTime = DateTime.UtcNow;
            _logger.LogError(ex, "全量扫描食材提醒时发生异常");
            throw;
        }

        return result;
    }

    private async Task<(bool StatusUpdated, int NearExpiryAlertsCreated, int ExpiredAlertsCreated, int AlertsRemoved, int AlertsKept)> ProcessFoodItemAsync(FoodItem foodItem)
    {
        var statusUpdated = false;
        var nearExpiryAlertsCreated = 0;
        var expiredAlertsCreated = 0;
        var alertsRemoved = 0;
        var alertsKept = 0;

        if (foodItem.Status == FoodStatus.Consumed)
        {
            var removed = await RemoveAllSystemAlertsForFoodItemAsync(foodItem.Id);
            alertsRemoved += removed;
            return (statusUpdated, nearExpiryAlertsCreated, expiredAlertsCreated, alertsRemoved, alertsKept);
        }

        var newStatus = FoodStatusHelper.CalculateStatus(foodItem.ExpiryDate, foodItem.Quantity);
        if (newStatus != foodItem.Status && foodItem.Status != FoodStatus.Consumed)
        {
            foodItem.Status = newStatus;
            foodItem.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.FoodItems.UpdateAsync(foodItem);
            statusUpdated = true;
        }

        var daysToExpiry = (foodItem.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
        var needsNearExpiryAlert = daysToExpiry <= 3 && daysToExpiry >= 0;
        var needsExpiredAlert = daysToExpiry < 0;

        var householdMembers = await _unitOfWork.HouseholdMembers
            .FindAsync(m => m.HouseholdId == foodItem.HouseholdId);

        var existingAlerts = (await _unitOfWork.ExpiryAlerts
            .FindAsync(a => a.FoodItemId == foodItem.Id && (a.AlertType == AlertType.NearExpiry || a.AlertType == AlertType.Expired)))
            .ToList();

        if (needsNearExpiryAlert || needsExpiredAlert)
        {
            var targetAlertType = needsExpiredAlert ? AlertType.Expired : AlertType.NearExpiry;
            var otherAlertType = needsExpiredAlert ? AlertType.NearExpiry : AlertType.Expired;

            foreach (var member in householdMembers)
            {
                var otherTypeAlert = existingAlerts.FirstOrDefault(a => a.UserId == member.UserId && a.AlertType == otherAlertType);
                if (otherTypeAlert != null)
                {
                    await _unitOfWork.ExpiryAlerts.DeleteAsync(otherTypeAlert.Id);
                    alertsRemoved++;
                    existingAlerts.Remove(otherTypeAlert);
                }

                var existingAlert = existingAlerts.FirstOrDefault(a => a.UserId == member.UserId && a.AlertType == targetAlertType);
                if (existingAlert == null)
                {
                    var alert = new ExpiryAlert
                    {
                        FoodItemId = foodItem.Id,
                        UserId = member.UserId,
                        AlertType = targetAlertType,
                        AlertDate = DateTime.UtcNow,
                        IsRead = false
                    };
                    await _unitOfWork.ExpiryAlerts.AddAsync(alert);

                    if (targetAlertType == AlertType.NearExpiry)
                    {
                        nearExpiryAlertsCreated++;
                    }
                    else
                    {
                        expiredAlertsCreated++;
                    }
                }
                else
                {
                    alertsKept++;
                }
            }
        }
        else
        {
            foreach (var alert in existingAlerts)
            {
                await _unitOfWork.ExpiryAlerts.DeleteAsync(alert.Id);
                alertsRemoved++;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return (statusUpdated, nearExpiryAlertsCreated, expiredAlertsCreated, alertsRemoved, alertsKept);
    }

    private async Task<int> RemoveAllSystemAlertsForFoodItemAsync(int foodItemId)
    {
        var alerts = await _unitOfWork.ExpiryAlerts
            .FindAsync(a => a.FoodItemId == foodItemId && (a.AlertType == AlertType.NearExpiry || a.AlertType == AlertType.Expired));

        var count = 0;
        foreach (var alert in alerts)
        {
            await _unitOfWork.ExpiryAlerts.DeleteAsync(alert.Id);
            count++;
        }

        await _unitOfWork.SaveChangesAsync();
        return count;
    }
}
