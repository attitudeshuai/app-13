using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly FridgeWatchDbContext _context;
    private readonly DbSet<AuditLog> _dbSet;

    public AuditLogRepository(FridgeWatchDbContext context)
    {
        _context = context;
        _dbSet = context.Set<AuditLog>();
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        await _dbSet.AddAsync(auditLog);
        return auditLog;
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(QueryParameters parameters)
    {
        var query = _dbSet.OrderByDescending(a => a.OperatedAt).AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByHouseholdIdAsync(int householdId, QueryParameters parameters)
    {
        var query = _dbSet
            .Where(a => a.HouseholdId == householdId)
            .OrderByDescending(a => a.OperatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByEntityTypeAsync(string entityType, QueryParameters parameters)
    {
        var query = _dbSet
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.OperatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetByOperatorIdAsync(int operatorId, QueryParameters parameters)
    {
        var query = _dbSet
            .Where(a => a.OperatorId == operatorId)
            .OrderByDescending(a => a.OperatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<PagedResult<AuditLog>> GetFilteredAsync(int? householdId, string? entityType, string? action, int? operatorId, QueryParameters parameters)
    {
        var query = _dbSet.AsQueryable();

        if (householdId.HasValue)
        {
            query = query.Where(a => a.HouseholdId == householdId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (operatorId.HasValue)
        {
            query = query.Where(a => a.OperatorId == operatorId.Value);
        }

        query = query.OrderByDescending(a => a.OperatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }
}
