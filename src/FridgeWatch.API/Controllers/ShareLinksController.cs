using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/sharelinks")]
[Authorize]
public class ShareLinksController : ApiControllerBase
{
    private readonly IShareService _shareService;

    public ShareLinksController(IShareService shareService)
    {
        _shareService = shareService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareLinkDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _shareService.CreateShareLinkAsync(dto, userId);
        return Success(result, "分享链接创建成功");
    }

    [HttpGet("household/{householdId}")]
    public async Task<IActionResult> GetByHouseholdId(
        int householdId,
        [FromQuery] QueryParametersDto parameters)
    {
        var userId = GetCurrentUserId();
        var result = await _shareService.GetShareLinksByHouseholdIdAsync(householdId, parameters, userId);
        return Success(result, "获取成功");
    }

    [HttpPatch("{id}/revoke")]
    public async Task<IActionResult> Revoke(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _shareService.RevokeShareLinkAsync(id, userId);
        return Success(result, "分享链接已撤销");
    }
}
