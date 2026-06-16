using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class StatsService : IStatsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StatsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StatsOverviewDto> GetOverviewAsync(int? householdId = null, int? userId = null)
    {
        var result = new StatsOverviewDto();

        List<int> householdIds = new();

        if (householdId.HasValue)
        {
            householdIds.Add(householdId.Value);
        }
        else if (userId.HasValue)
        {
            var members = await _unitOfWork.HouseholdMembers.FindAsync(m => m.UserId == userId.Value);
            householdIds = members.Select(m => m.HouseholdId).ToList();
        }

        var hasFilter = householdIds.Any();

        if (hasFilter)
        {
            result.TotalHouseholds = householdIds.Count;

            var allFoodItems = new List<FoodItem>();
            var allConsumptionRecords = new List<ConsumptionRecord>();

            foreach (var hId in householdIds)
            {
                var foodItems = await _unitOfWork.FoodItems.FindAsync(f => f.HouseholdId == hId);
                allFoodItems.AddRange(foodItems);

                var records = await _unitOfWork.ConsumptionRecords.FindAsync(c => c.FoodItem!.HouseholdId == hId);
                allConsumptionRecords.AddRange(records);
            }

            result.TotalFoodItems = allFoodItems.Count;
            result.FreshCount = allFoodItems.Count(f => f.Status == FoodStatus.Fresh);
            result.NearExpiryCount = allFoodItems.Count(f => f.Status == FoodStatus.NearExpiry);
            result.ExpiredCount = allFoodItems.Count(f => f.Status == FoodStatus.Expired);
            result.ConsumedCount = allFoodItems.Count(f => f.Status == FoodStatus.Consumed);
            result.TotalConsumedQuantity = allConsumptionRecords.Sum(c => c.ConsumedQuantity);

            result.CategoryStats = allFoodItems
                .GroupBy(f => f.Category)
                .Select(g => new FoodCategoryStatDto { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            result.LocationStats = allFoodItems
                .GroupBy(f => f.StorageLocation.ToString())
                .Select(g => new FoodLocationStatDto { Location = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (userId.HasValue)
            {
                result.UnreadAlerts = await _unitOfWork.ExpiryAlerts.GetUnreadCountAsync(userId.Value);
            }
        }
        else
        {
            result.TotalHouseholds = await _unitOfWork.Households.CountAsync(h => true);
            result.TotalFoodItems = await _unitOfWork.FoodItems.CountAsync(f => true);
            result.FreshCount = await _unitOfWork.FoodItems.CountAsync(f => f.Status == FoodStatus.Fresh);
            result.NearExpiryCount = await _unitOfWork.FoodItems.CountAsync(f => f.Status == FoodStatus.NearExpiry);
            result.ExpiredCount = await _unitOfWork.FoodItems.CountAsync(f => f.Status == FoodStatus.Expired);
            result.ConsumedCount = await _unitOfWork.FoodItems.CountAsync(f => f.Status == FoodStatus.Consumed);
        }

        return result;
    }

    public async Task<StatsTrendDto> GetTrendAsync(StatsTrendQueryDto query, int? userId = null)
    {
        var result = new StatsTrendDto();

        var startDate = query.StartDate.Date;
        var endDate = query.EndDate.Date;

        List<int> householdIds = new();

        if (query.HouseholdId.HasValue)
        {
            householdIds.Add(query.HouseholdId.Value);
        }
        else if (userId.HasValue)
        {
            var members = await _unitOfWork.HouseholdMembers.FindAsync(m => m.UserId == userId.Value);
            householdIds = members.Select(m => m.HouseholdId).ToList();
        }

        var totalDays = (endDate - startDate).Days + 1;

        var foodAddedTrend = new List<TrendItemDto>();
        var consumptionTrend = new List<TrendItemDto>();
        var expiryTrend = new List<TrendItemDto>();

        var hasFilter = householdIds.Any();

        for (int i = 0; i < totalDays; i++)
        {
            var currentDate = startDate.AddDays(i);
            var nextDate = currentDate.AddDays(1);

            int addedCount = 0;
            int consumptionCount = 0;
            int expiryCount = 0;

            if (hasFilter)
            {
                foreach (var hId in householdIds)
                {
                    var foodItems = await _unitOfWork.FoodItems.FindAsync(f =>
                        f.HouseholdId == hId &&
                        f.CreatedAt >= currentDate &&
                        f.CreatedAt < nextDate);
                    addedCount += foodItems.Count;

                    var records = await _unitOfWork.ConsumptionRecords.FindAsync(c =>
                        c.FoodItem!.HouseholdId == hId &&
                        c.ConsumedAt >= currentDate &&
                        c.ConsumedAt < nextDate);
                    consumptionCount += records.Count;

                    var expiring = await _unitOfWork.FoodItems.FindAsync(f =>
                        f.HouseholdId == hId &&
                        f.ExpiryDate.Date == currentDate);
                    expiryCount += expiring.Count;
                }
            }

            foodAddedTrend.Add(new TrendItemDto { Date = currentDate, Value = addedCount });
            consumptionTrend.Add(new TrendItemDto { Date = currentDate, Value = consumptionCount });
            expiryTrend.Add(new TrendItemDto { Date = currentDate, Value = expiryCount });
        }

        result.FoodAddedTrend = foodAddedTrend;
        result.ConsumptionTrend = consumptionTrend;
        result.ExpiryTrend = expiryTrend;

        return result;
    }
}
