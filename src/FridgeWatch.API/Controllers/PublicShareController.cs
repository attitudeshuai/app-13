using Microsoft.AspNetCore.Mvc;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/public/share")]
[ApiController]
[Produces("application/json")]
public class PublicShareController : ControllerBase
{
    private readonly IShareService _shareService;

    public PublicShareController(IShareService shareService)
    {
        _shareService = shareService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetSharedFoodItems(string token)
    {
        var result = await _shareService.GetSharedFoodItemsAsync(token);
        return Ok(ApiResponse<SharedFoodItemsDto>.Success(result, "获取成功"));
    }
}
