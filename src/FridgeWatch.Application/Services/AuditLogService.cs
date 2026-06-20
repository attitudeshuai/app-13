using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task LogAsync(string entityType, int entityId, string action, int operatorId, string operatorName, int? householdId, string summary, string? detail = null)
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OperatorId = operatorId,
            OperatorName = operatorName,
            HouseholdId = householdId,
            Summary = summary,
            Detail = detail,
            OperatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<AuditLogDto>> GetListAsync(AuditLogQueryParametersDto parameters)
    {
        var queryParams = _mapper.Map<Domain.Common.QueryParameters>(parameters);
        var result = await _unitOfWork.AuditLogs.GetFilteredAsync(
            parameters.HouseholdId,
            parameters.EntityType,
            parameters.Action,
            parameters.OperatorId,
            queryParams);

        return _mapper.Map<PagedResultDto<AuditLogDto>>(result);
    }
}
