using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/consumptionrecords")]
[Authorize]
public class ConsumptionRecordsController : ApiControllerBase
{
    private readonly IConsumptionRecordService _consumptionRecordService;

    public ConsumptionRecordsController(IConsumptionRecordService consumptionRecordService)
    {
        _consumptionRecordService = consumptionRecordService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] int? foodItemId = null,
        [FromQuery] int? householdId = null)
    {
        var result = await _consumptionRecordService.GetListAsync(parameters, foodItemId, householdId);
        return Success(result, "获取成功");
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] QueryParametersDto parameters)
    {
        var userId = GetCurrentUserId();
        var result = await _consumptionRecordService.GetMineAsync(userId, parameters);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _consumptionRecordService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsumptionRecordCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _consumptionRecordService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ConsumptionRecordDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConsumptionRecordUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _consumptionRecordService.UpdateAsync(id, dto, userId);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        await _consumptionRecordService.DeleteAsync(id, userId);
        return Success("删除成功");
    }
}
