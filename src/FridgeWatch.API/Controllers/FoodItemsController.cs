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
        [FromQuery] FoodItemQueryParametersDto parameters,
        [FromQuery] int? householdId = null)
    {
        var userId = GetCurrentUserId();
        var result = await _foodItemService.GetListAsync(parameters, householdId, userId);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _foodItemService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] FoodItemCreateDto dto, IFormFile? photo)
    {
        var userId = GetCurrentUserId();

        Stream? photoStream = null;
        string? photoFileName = null;
        if (photo != null && photo.Length > 0)
        {
            photoStream = new MemoryStream();
            await photo.CopyToAsync(photoStream);
            photoFileName = photo.FileName;
        }

        var result = await _foodItemService.CreateAsync(dto, userId, photoStream, photoFileName);
        photoStream?.Dispose();

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<FoodItemDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Update(int id, [FromForm] FoodItemUpdateDto dto, IFormFile? photo)
    {
        var userId = GetCurrentUserId();

        Stream? photoStream = null;
        string? photoFileName = null;
        if (photo != null && photo.Length > 0)
        {
            photoStream = new MemoryStream();
            await photo.CopyToAsync(photoStream);
            photoFileName = photo.FileName;
        }

        var result = await _foodItemService.UpdateAsync(id, dto, userId, photoStream, photoFileName);
        photoStream?.Dispose();

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

    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var templateBytes = await _foodItemService.DownloadTemplateAsync();
        var fileName = "食材批量导入模板.xlsx";
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(templateBytes, contentType, fileName);
    }

    [HttpPost("import")]
    public async Task<IActionResult> BulkImport(
        [FromQuery] int householdId,
        IFormFile file)
    {
        var userId = GetCurrentUserId();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var result = await _foodItemService.BulkImportAsync(householdId, stream, file.FileName, userId);
        return Success(result, $"导入完成：新增 {result.CreatedCount} 条，更新 {result.UpdatedCount} 条，失败 {result.FailedCount} 条");
    }
}
