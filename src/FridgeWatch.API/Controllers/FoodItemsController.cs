using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/fooditems")]
[Authorize]
public class FoodItemsController : ApiControllerBase
{
    private readonly IFoodItemService _foodItemService;

    public FoodItemsController(IFoodItemService foodItemService)
    {
        _foodItemService = foodItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] int? householdId = null)
    {
        var result = await _foodItemService.GetListAsync(parameters, householdId);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _foodItemService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FoodItemCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _foodItemService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<FoodItemDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] FoodItemUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _foodItemService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _foodItemService.DeleteAsync(id, userId);
        return Success("删除成功");
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] FoodItemStatusUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _foodItemService.UpdateStatusAsync(id, dto.Status, userId);
        return Success(result, "状态更新成功");
    }
}
