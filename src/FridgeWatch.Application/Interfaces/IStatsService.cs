using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Interfaces;

public interface IStatsService
{
    Task<StatsOverviewDto> GetOverviewAsync(int? householdId = null, int? userId = null);
    Task<StatsTrendDto> GetTrendAsync(StatsTrendQueryDto query, int? userId = null);
}
