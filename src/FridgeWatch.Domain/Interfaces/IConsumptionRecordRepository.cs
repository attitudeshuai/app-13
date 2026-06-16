using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IConsumptionRecordRepository : IRepository<ConsumptionRecord, int>
{
    Task<PagedResult<ConsumptionRecord>> GetByUserIdAsync(int userId, QueryParameters parameters);
    Task<PagedResult<ConsumptionRecord>> GetByFoodItemIdAsync(int foodItemId, QueryParameters parameters);
    Task<PagedResult<ConsumptionRecord>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters);
    Task<decimal> GetTotalConsumedQuantityByFoodItemAsync(int foodItemId);
}
