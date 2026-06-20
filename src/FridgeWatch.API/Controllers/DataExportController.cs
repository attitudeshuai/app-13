using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/data-export")]
[Authorize]
public class DataExportController : ApiControllerBase
{
    private readonly IDataExportService _dataExportService;

    public DataExportController(IDataExportService dataExportService)
    {
        _dataExportService = dataExportService;
    }

    [HttpGet]
    public async Task<IActionResult> Export(
        [FromQuery] int householdId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetCurrentUserId();

        var dto = new DataExportRequestDto
        {
            HouseholdId = householdId,
            StartDate = startDate,
            EndDate = endDate
        };

        var fileBytes = await _dataExportService.ExportAsync(dto, userId);

        var fileName = $"FridgeWatch_数据导出_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.xlsx";
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(fileBytes, contentType, fileName);
    }
}
