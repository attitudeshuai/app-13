using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/stats")]
[Authorize]
public class StatsController : ApiControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] StatsOverviewQueryDto query)
    {
        var userId = GetCurrentUserId();
        var result = await _statsService.GetOverviewAsync(query, userId);
        return Success(result, "获取成功");
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend([FromQuery] StatsTrendQueryDto query)
    {
        var userId = GetCurrentUserId();
        var result = await _statsService.GetTrendAsync(query, userId);
        return Success(result, "获取成功");
    }
}
