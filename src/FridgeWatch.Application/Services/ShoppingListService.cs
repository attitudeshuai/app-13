using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IExpiryAlertSyncService _alertSyncService;

    public ShoppingListService(IUnitOfWork unitOfWork, IMapper mapper, IExpiryAlertSyncService alertSyncService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _alertSyncService = alertSyncService;
    }

    public async Task<PagedResultDto<ShoppingListDto>> GetListAsync(QueryParametersDto parameters, int? householdId = null, int? userId = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);

        var resolvedHouseholdId = await ResolveHouseholdIdAsync(householdId, userId);
        if (resolvedHouseholdId.HasValue)
        {
            var result = await _unitOfWork.ShoppingLists.GetByHouseholdIdAsync(resolvedHouseholdId.Value, queryParams);
            return _mapper.Map<PagedResultDto<ShoppingListDto>>(result);
        }

        var allResult = await _unitOfWork.ShoppingLists.GetPagedAsync(queryParams);
        return _mapper.Map<PagedResultDto<ShoppingListDto>>(allResult);
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

    public async Task<ShoppingListDto> GetByIdAsync(int id)
    {
        var shoppingList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(id);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        return _mapper.Map<ShoppingListDto>(shoppingList);
    }

    public async Task<ShoppingListDto> CreateAsync(ShoppingListCreateDto dto, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(dto.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法创建采购清单");
        }

        var shoppingList = _mapper.Map<ShoppingList>(dto);

        await _unitOfWork.ShoppingLists.AddAsync(shoppingList);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShoppingListDto>(shoppingList);
    }

    public async Task<ShoppingListDto> UpdateAsync(int id, ShoppingListUpdateDto dto, int userId)
    {
        var shoppingList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(id);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改采购清单");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("已完成的采购清单无法修改");
        }

        _mapper.Map(dto, shoppingList);

        if (dto.Items != null)
        {
            foreach (var existingItem in shoppingList.Items.ToList())
            {
                await _unitOfWork.ShoppingListItems.DeleteAsync(existingItem.Id);
            }

            foreach (var itemDto in dto.Items)
            {
                var item = _mapper.Map<ShoppingListItem>(itemDto);
                item.ShoppingListId = shoppingList.Id;
                await _unitOfWork.ShoppingListItems.AddAsync(item);
            }
        }

        await _unitOfWork.ShoppingLists.UpdateAsync(shoppingList);
        await _unitOfWork.SaveChangesAsync();

        var updatedList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(id);
        return _mapper.Map<ShoppingListDto>(updatedList);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var shoppingList = await _unitOfWork.ShoppingLists.GetByIdAsync(id);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法删除采购清单");
        }

        await _unitOfWork.ShoppingLists.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ShoppingListDto> AddItemAsync(int shoppingListId, ShoppingListItemCreateDto dto, int userId)
    {
        var shoppingList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(shoppingListId);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法添加清单项");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("已完成的采购清单无法添加项");
        }

        var item = _mapper.Map<ShoppingListItem>(dto);
        item.ShoppingListId = shoppingListId;

        await _unitOfWork.ShoppingListItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        var updatedList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(shoppingListId);
        return _mapper.Map<ShoppingListDto>(updatedList);
    }

    public async Task<ShoppingListDto> UpdateItemAsync(int itemId, ShoppingListItemUpdateDto dto, int userId)
    {
        var item = await _unitOfWork.ShoppingListItems.GetByIdAsync(itemId);
        if (item == null)
        {
            throw new BusinessException("清单项不存在");
        }

        var shoppingList = await _unitOfWork.ShoppingLists.GetByIdAsync(item.ShoppingListId);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改清单项");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("已完成的采购清单无法修改项");
        }

        _mapper.Map(dto, item);

        await _unitOfWork.ShoppingListItems.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        var updatedList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(item.ShoppingListId);
        return _mapper.Map<ShoppingListDto>(updatedList);
    }

    public async Task<ShoppingListDto> RemoveItemAsync(int itemId, int userId)
    {
        var item = await _unitOfWork.ShoppingListItems.GetByIdAsync(itemId);
        if (item == null)
        {
            throw new BusinessException("清单项不存在");
        }

        var shoppingList = await _unitOfWork.ShoppingLists.GetByIdAsync(item.ShoppingListId);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法删除清单项");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("已完成的采购清单无法删除项");
        }

        await _unitOfWork.ShoppingListItems.DeleteAsync(itemId);
        await _unitOfWork.SaveChangesAsync();

        var updatedList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(item.ShoppingListId);
        return _mapper.Map<ShoppingListDto>(updatedList);
    }

    public async Task<ShoppingListDto> ToggleItemPurchasedAsync(int itemId, bool isPurchased, int userId)
    {
        var item = await _unitOfWork.ShoppingListItems.GetByIdAsync(itemId);
        if (item == null)
        {
            throw new BusinessException("清单项不存在");
        }

        var shoppingList = await _unitOfWork.ShoppingLists.GetByIdAsync(item.ShoppingListId);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改清单项");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("已完成的采购清单无法修改");
        }

        item.IsPurchased = isPurchased;
        await _unitOfWork.ShoppingListItems.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        var updatedList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(item.ShoppingListId);
        return _mapper.Map<ShoppingListDto>(updatedList);
    }

    public async Task<List<FoodItemDto>> ConvertToFoodItemsAsync(ShoppingListConvertDto dto, int userId)
    {
        var shoppingList = await _unitOfWork.ShoppingLists.GetWithItemsByIdAsync(dto.ShoppingListId);
        if (shoppingList == null)
        {
            throw new BusinessException("采购清单不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(shoppingList.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法转换采购清单");
        }

        if (shoppingList.IsCompleted)
        {
            throw new BusinessException("该采购清单已完成转换");
        }

        var itemsToConvert = dto.PurchaseAll
            ? shoppingList.Items.ToList()
            : shoppingList.Items.Where(i => i.IsPurchased).ToList();

        if (!itemsToConvert.Any())
        {
            throw new BusinessException("没有可转换的清单项");
        }

        var createdFoodItems = new List<FoodItem>();
        var itemOverridesDict = dto.ItemOverrides?.ToDictionary(o => o.ItemId) ?? new Dictionary<int, ShoppingListItemConvertDto>();

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var purchaseDate = DateTime.UtcNow;

            foreach (var item in itemsToConvert)
            {
                ShoppingListItemConvertDto? itemOverride = null;
                itemOverridesDict.TryGetValue(item.Id, out itemOverride);

                var storageLocation = itemOverride?.StorageLocation
                    ?? item.StorageLocation
                    ?? StorageLocation.Fridge;

                var expiryDays = itemOverride?.ExpiryDays
                    ?? item.ExpiryDays
                    ?? 7;

                var expiryDate = purchaseDate.AddDays(expiryDays);

                var foodItem = new FoodItem
                {
                    HouseholdId = shoppingList.HouseholdId,
                    Name = item.Name,
                    Category = item.Category,
                    StorageLocation = storageLocation,
                    PurchaseDate = purchaseDate,
                    ExpiryDate = expiryDate,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    PhotoUrl = itemOverride?.PhotoUrl,
                    Status = FoodStatusHelper.CalculateStatus(expiryDate, item.Quantity)
                };

                await _unitOfWork.FoodItems.AddAsync(foodItem);
                createdFoodItems.Add(foodItem);
            }

            shoppingList.IsCompleted = true;
            shoppingList.CompletedAt = DateTime.UtcNow;
            await _unitOfWork.ShoppingLists.UpdateAsync(shoppingList);

            await _unitOfWork.SaveChangesAsync();

            foreach (var foodItem in createdFoodItems)
            {
                await _alertSyncService.SyncAlertsForFoodItemAsync(foodItem);
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return _mapper.Map<List<FoodItemDto>>(createdFoodItems);
    }
}
