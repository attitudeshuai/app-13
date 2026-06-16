using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Domain.Interfaces;

public interface IFoodItemRepository : IRepository<FoodItem, int>
{
    Task<PagedResult<FoodItem>> GetFilteredAsync(FoodItemQueryParameters parameters, int? householdId = null);
    Task<List<FoodItem>> GetExpiringSoonAsync(int days);
    Task<List<FoodItem>> GetExpiredAsync();
    Task UpdateStatusAsync(int id, FoodStatus status);
}
