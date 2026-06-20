using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string entityType, int entityId, string action, int operatorId, string operatorName, int? householdId, string summary, string? detail = null);
    Task<PagedResultDto<AuditLogDto>> GetListAsync(AuditLogQueryParametersDto parameters);
}
