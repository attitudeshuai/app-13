using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IShoppingListRepository : IRepository<ShoppingList, int>
{
    Task<PagedResult<ShoppingList>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters);
    Task<ShoppingList?> GetWithItemsByIdAsync(int id);
}
