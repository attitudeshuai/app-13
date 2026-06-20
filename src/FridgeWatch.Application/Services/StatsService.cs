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

    private static (DateTime StartDate, DateTime EndDate) ResolveDateRange(TimeRangeType timeRange, DateTime? startDate, DateTime? endDate)
    {
        var today = DateTime.Today;

        return timeRange switch
        {
            TimeRangeType.Last7Days => (today.AddDays(-6), today),
            TimeRangeType.Last30Days => (today.AddDays(-29), today),
            TimeRangeType.Custom => (startDate ?? today.AddDays(-6), endDate ?? today),
            _ => (today.AddDays(-6), today)
        };
    }

    public async Task<StatsOverviewDto> GetOverviewAsync(StatsOverviewQueryDto query, int? userId = null)
    {
        var result = new StatsOverviewDto();
        var (startDate, endDate) = ResolveDateRange(query.TimeRange, query.StartDate, query.EndDate);
        var startDateUtc = startDate.Date;
        var endDateUtc = endDate.Date.AddDays(1);

        List<int> householdIds = new();

        var resolvedHouseholdId = await ResolveHouseholdIdAsync(query.HouseholdId, userId);
        if (resolvedHouseholdId.HasValue)
        {
            householdIds.Add(resolvedHouseholdId.Value);
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
                var foodItems = await _unitOfWork.FoodItems.FindAsync(f =>
                    f.HouseholdId == hId &&
                    f.CreatedAt >= startDateUtc &&
                    f.CreatedAt < endDateUtc);
                allFoodItems.AddRange(foodItems);

                var records = await _unitOfWork.ConsumptionRecords.FindAsync(c =>
                    c.FoodItem!.HouseholdId == hId &&
                    c.ConsumedAt >= startDateUtc &&
                    c.ConsumedAt < endDateUtc);
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

            var foodItemsInRange = await _unitOfWork.FoodItems.FindAsync(f =>
                f.CreatedAt >= startDateUtc &&
                f.CreatedAt < endDateUtc);
            result.TotalFoodItems = foodItemsInRange.Count;
            result.FreshCount = foodItemsInRange.Count(f => f.Status == FoodStatus.Fresh);
            result.NearExpiryCount = foodItemsInRange.Count(f => f.Status == FoodStatus.NearExpiry);
            result.ExpiredCount = foodItemsInRange.Count(f => f.Status == FoodStatus.Expired);
            result.ConsumedCount = foodItemsInRange.Count(f => f.Status == FoodStatus.Consumed);

            var recordsInRange = await _unitOfWork.ConsumptionRecords.FindAsync(c =>
                c.ConsumedAt >= startDateUtc &&
                c.ConsumedAt < endDateUtc);
            result.TotalConsumedQuantity = recordsInRange.Sum(c => c.ConsumedQuantity);
        }

        return result;
    }

    public async Task<StatsTrendDto> GetTrendAsync(StatsTrendQueryDto query, int? userId = null)
    {
        var result = new StatsTrendDto();
        var (resolvedStart, resolvedEnd) = ResolveDateRange(query.TimeRange, query.StartDate, query.EndDate);

        var startDate = resolvedStart.Date;
        var endDate = resolvedEnd.Date;

        List<int> householdIds = new();

        var resolvedHouseholdId = await ResolveHouseholdIdAsync(query.HouseholdId, userId);
        if (resolvedHouseholdId.HasValue)
        {
            householdIds.Add(resolvedHouseholdId.Value);
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

    public async Task<List<MemberActivityStatsDto>> GetMemberActivityAsync(MemberActivityQueryDto query, int? userId = null)
    {
        var (startDate, endDate) = ResolveDateRange(query.TimeRange, query.StartDate, query.EndDate);
        var startDateUtc = startDate.Date;
        var endDateUtc = endDate.Date.AddDays(1);

        if (userId.HasValue)
        {
            var isMember = await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(query.HouseholdId, userId.Value);
            if (!isMember)
            {
                throw new UnauthorizedAccessException("您不是该家庭的成员，无法查看成员活跃度统计");
            }
        }

        var householdMembers = await _unitOfWork.HouseholdMembers.FindAsync(m => m.HouseholdId == query.HouseholdId);

        var result = new List<MemberActivityStatsDto>();

        foreach (var member in householdMembers)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
            if (user == null) continue;

            var addedFoodItems = await _unitOfWork.FoodItems.FindAsync(f =>
                f.HouseholdId == query.HouseholdId &&
                f.CreatedByUserId == member.UserId &&
                f.CreatedAt >= startDateUtc &&
                f.CreatedAt < endDateUtc);

            var consumptionRecords = await _unitOfWork.ConsumptionRecords.FindAsync(c =>
                c.FoodItem!.HouseholdId == query.HouseholdId &&
                c.UserId == member.UserId &&
                c.ConsumedAt >= startDateUtc &&
                c.ConsumedAt < endDateUtc);

            var handledAlerts = await _unitOfWork.ExpiryAlerts.FindAsync(a =>
                a.FoodItem!.HouseholdId == query.HouseholdId &&
                a.UserId == member.UserId &&
                a.IsRead &&
                a.UpdatedAt >= startDateUtc &&
                a.UpdatedAt < endDateUtc);

            DateTime? lastActiveAt = null;

            if (addedFoodItems.Any())
            {
                var latestFood = addedFoodItems.Max(f => f.CreatedAt);
                if (lastActiveAt == null || latestFood > lastActiveAt)
                    lastActiveAt = latestFood;
            }

            if (consumptionRecords.Any())
            {
                var latestConsumption = consumptionRecords.Max(c => c.ConsumedAt);
                if (lastActiveAt == null || latestConsumption > lastActiveAt)
                    lastActiveAt = latestConsumption;
            }

            if (handledAlerts.Any())
            {
                var latestAlert = handledAlerts.Max(a => a.UpdatedAt);
                if (lastActiveAt == null || latestAlert > lastActiveAt)
                    lastActiveAt = latestAlert;
            }

            result.Add(new MemberActivityStatsDto
            {
                UserId = member.UserId,
                Username = user.Username,
                Avatar = user.Avatar,
                AddedFoodCount = addedFoodItems.Count,
                ConsumptionCount = consumptionRecords.Count,
                HandledAlertsCount = handledAlerts.Count,
                LastActiveAt = lastActiveAt
            });
        }

        return result.OrderByDescending(x => x.TotalActivities).ToList();
    }

    private async Task<int?> ResolveHouseholdIdAsync(int? householdId, int? userId)
    {
        if (householdId.HasValue)
        {
            return householdId.Value;
        }

        if (userId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user != null && user.DefaultHouseholdId.HasValue)
            {
                return user.DefaultHouseholdId.Value;
            }
        }

        return null;
    }
}
