using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IShoppingListService
{
    Task<PagedResultDto<ShoppingListDto>> GetListAsync(QueryParametersDto parameters, int? householdId = null, int? userId = null);
    Task<ShoppingListDto> GetByIdAsync(int id);
    Task<ShoppingListDto> CreateAsync(ShoppingListCreateDto dto, int userId);
    Task<ShoppingListDto> UpdateAsync(int id, ShoppingListUpdateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
    Task<ShoppingListDto> AddItemAsync(int shoppingListId, ShoppingListItemCreateDto dto, int userId);
    Task<ShoppingListDto> UpdateItemAsync(int itemId, ShoppingListItemUpdateDto dto, int userId);
    Task<ShoppingListDto> RemoveItemAsync(int itemId, int userId);
    Task<List<FoodItemDto>> ConvertToFoodItemsAsync(ShoppingListConvertDto dto, int userId);
    Task<ShoppingListDto> ToggleItemPurchasedAsync(int itemId, bool isPurchased, int userId);
}
