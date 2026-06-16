using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/expiryalerts")]
[Authorize]
public class ExpiryAlertsController : ApiControllerBase
{
    private readonly IExpiryAlertService _expiryAlertService;

    public ExpiryAlertsController(IExpiryAlertService expiryAlertService)
    {
        _expiryAlertService = expiryAlertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] int? foodItemId = null)
    {
        var result = await _expiryAlertService.GetListAsync(parameters, foodItemId);
        return Success(result, "获取成功");
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] QueryParametersDto parameters)
    {
        var userId = GetCurrentUserId();
        var result = await _expiryAlertService.GetMineAsync(userId, parameters);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _expiryAlertService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExpiryAlertCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _expiryAlertService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ExpiryAlertDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExpiryAlertUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _expiryAlertService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _expiryAlertService.DeleteAsync(id, userId);
        return Success("删除成功");
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _expiryAlertService.GetUnreadCountAsync(userId);
        return Success(new { Count = result }, "获取成功");
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        await _expiryAlertService.MarkAsReadAsync(id, userId);
        return Success("标记已读成功");
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        await _expiryAlertService.MarkAllAsReadAsync(userId);
        return Success("全部已读成功");
    }
}
