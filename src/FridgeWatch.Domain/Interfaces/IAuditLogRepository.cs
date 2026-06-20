using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Entities;

namespace FridgeWatch.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task<PagedResult<AuditLog>> GetPagedAsync(QueryParameters parameters);
    Task<PagedResult<AuditLog>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters);
    Task<PagedResult<AuditLog>> GetByEntityTypeAsync(string entityType, QueryParameters parameters);
    Task<PagedResult<AuditLog>> GetByOperatorIdAsync(int operatorId, QueryParameters parameters);
    Task<PagedResult<AuditLog>> GetFilteredAsync(int? householdId, string? entityType, string? action, int? operatorId, QueryParameters parameters);
}
