using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Domain.Interfaces;

public interface IShoppingListItemRepository : IRepository<ShoppingListItem, int>
{
    Task<List<ShoppingListItem>> GetByShoppingListIdAsync(int shoppingListId);
}
