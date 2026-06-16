using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using FridgeWatch.Application.DTOs;

namespace FridgeWatch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("用户未登录");
        }
        return userId;
    }

    protected IActionResult Success<T>(T data, string message = "操作成功")
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    protected IActionResult Success(string message = "操作成功")
    {
        return Ok(ApiResponse.Success(message));
    }

    protected IActionResult Fail(string message, int code = 400)
    {
        return StatusCode(code, ApiResponse.Fail(message, code));
    }
}
