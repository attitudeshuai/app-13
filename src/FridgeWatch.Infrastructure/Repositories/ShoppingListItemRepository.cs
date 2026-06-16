using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class ShoppingListItemRepository : Repository<ShoppingListItem, int>, IShoppingListItemRepository
{
    public ShoppingListItemRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<List<ShoppingListItem>> GetByShoppingListIdAsync(int shoppingListId)
    {
        return await _dbSet
            .Where(sli => sli.ShoppingListId == shoppingListId)
            .OrderBy(sli => sli.CreatedAt)
            .ToListAsync();
    }
}
