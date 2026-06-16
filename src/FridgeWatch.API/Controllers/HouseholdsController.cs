using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/households")]
[Authorize]
public class HouseholdsController : ApiControllerBase
{
    private readonly IHouseholdService _householdService;

    public HouseholdsController(IHouseholdService householdService)
    {
        _householdService = householdService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] QueryParametersDto parameters)
    {
        var userId = GetCurrentUserId();
        var result = await _householdService.GetListAsync(parameters, userId);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _householdService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HouseholdCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _householdService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<HouseholdDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HouseholdUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _householdService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _householdService.DeleteAsync(id, userId);
        return Success("删除成功");
    }
}
