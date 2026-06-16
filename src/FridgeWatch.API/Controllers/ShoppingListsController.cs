using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.API.Controllers;

[Route("api/shoppinglists")]
[Authorize]
public class ShoppingListsController : ApiControllerBase
{
    private readonly IShoppingListService _shoppingListService;

    public ShoppingListsController(IShoppingListService shoppingListService)
    {
        _shoppingListService = shoppingListService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] int? householdId = null)
    {
        var result = await _shoppingListService.GetListAsync(parameters, householdId);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _shoppingListService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShoppingListCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ShoppingListDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShoppingListUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _shoppingListService.DeleteAsync(id, userId);
        return Success("删除成功");
    }

    [HttpPost("{shoppingListId}/items")]
    public async Task<IActionResult> AddItem(int shoppingListId, [FromBody] ShoppingListItemCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.AddItemAsync(shoppingListId, dto, userId);
        return Success(result, "添加成功");
    }

    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] ShoppingListItemUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.UpdateItemAsync(itemId, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.RemoveItemAsync(itemId, userId);
        return Success(result, "删除成功");
    }

    [HttpPatch("items/{itemId}/purchased")]
    public async Task<IActionResult> ToggleItemPurchased(int itemId, [FromBody] bool isPurchased)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.ToggleItemPurchasedAsync(itemId, isPurchased, userId);
        return Success(result, "更新成功");
    }

    [HttpPost("convert")]
    public async Task<IActionResult> ConvertToFoodItems([FromBody] ShoppingListConvertDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shoppingListService.ConvertToFoodItemsAsync(dto, userId);
        return Success(result, "转换成功，已生成食材记录");
    }
}
