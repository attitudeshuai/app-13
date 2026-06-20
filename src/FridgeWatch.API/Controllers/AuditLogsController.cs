using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/auditlogs")]
[Authorize]
public class AuditLogsController : ApiControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] AuditLogQueryParametersDto parameters)
    {
        var result = await _auditLogService.GetListAsync(parameters);
        return Success(result, "获取成功");
    }
}
