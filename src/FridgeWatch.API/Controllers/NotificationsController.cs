using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] NotificationQueryParametersDto? filter = null)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetMineAsync(userId, parameters, filter);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetByIdAsync(id, userId);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
    {
        var result = await _notificationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<NotificationDto>.Success(result, "创建成功"));
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchCreate([FromBody] NotificationBatchCreateDto dto)
    {
        var result = await _notificationService.BatchCreateAsync(dto);
        return Success(result, "批量创建成功");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] NotificationUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userId = GetCurrentUserId();
        await _notificationService.DeleteAsync(id, userId);
        return Success("删除成功");
    }

    [HttpPost("batch-delete")]
    public async Task<IActionResult> BatchDelete([FromBody] NotificationBatchDeleteDto dto)
    {
        var userId = GetCurrentUserId();
        await _notificationService.BatchDeleteAsync(dto.Ids, userId);
        return Success("批量删除成功");
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetUnreadCountAsync(userId);
        return Success(result, "获取成功");
    }

    [HttpGet("unread-count/total")]
    public async Task<IActionResult> GetTotalUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetTotalUnreadCountAsync(userId);
        return Success(new { Count = result }, "获取成功");
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(id, userId);
        return Success("标记已读成功");
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] NotificationType? type = null)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId, type);
        return Success("全部已读成功");
    }

    [HttpPut("batch-read")]
    public async Task<IActionResult> BatchMarkAsRead([FromBody] NotificationBatchMarkReadDto dto)
    {
        var userId = GetCurrentUserId();
        await _notificationService.BatchMarkAsReadAsync(dto.Ids, userId);
        return Success("批量已读成功");
    }

    [HttpPost("sync-expiry-alerts")]
    public async Task<IActionResult> SyncExpiryAlerts()
    {
        var userId = GetCurrentUserId();
        await _notificationService.SyncExpiryAlertsToNotificationsAsync(userId);
        return Success("同步成功");
    }
}
