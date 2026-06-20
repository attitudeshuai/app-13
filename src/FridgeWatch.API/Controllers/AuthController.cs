using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Success(result, "注册成功");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Success(result, "登录成功");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var result = await _authService.GetCurrentUserAsync(userId);
        return Success(result, "获取成功");
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.UpdateCurrentUserAsync(userId, dto);
        return Success(result, "更新成功");
    }

    [HttpPut("me/default-household")]
    [Authorize]
    public async Task<IActionResult> SetDefaultHousehold([FromBody] SetDefaultHouseholdDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.SetDefaultHouseholdAsync(userId, dto.HouseholdId);
        return Success(result, "设置默认家庭成功");
    }
}

public class SetDefaultHouseholdDto
{
    public int? HouseholdId { get; set; }
}
