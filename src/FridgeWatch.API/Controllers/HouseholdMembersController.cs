using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/householdmembers")]
[Authorize]
public class HouseholdMembersController : ApiControllerBase
{
    private readonly IHouseholdMemberService _householdMemberService;

    public HouseholdMembersController(IHouseholdMemberService householdMemberService)
    {
        _householdMemberService = householdMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] int? householdId = null)
    {
        var result = await _householdMemberService.GetListAsync(parameters, householdId);
        return Success(result, "获取成功");
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] QueryParametersDto parameters)
    {
        var userId = GetCurrentUserId();
        var result = await _householdMemberService.GetMineAsync(userId, parameters);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _householdMemberService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinHouseholdDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _householdMemberService.JoinHouseholdAsync(dto.InviteCode, userId);
        return Success(result, "加入成功");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HouseholdMemberUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _householdMemberService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _householdMemberService.DeleteAsync(id, userId);
        return Success("删除成功");
    }

    [HttpPost("leave/{householdId}")]
    public async Task<IActionResult> Leave(int householdId)
    {
        var userId = GetCurrentUserId();
        await _householdMemberService.LeaveHouseholdAsync(householdId, userId);
        return Success("退出成功");
    }
}
