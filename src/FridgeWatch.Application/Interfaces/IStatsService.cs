using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Interfaces;

public interface IStatsService
{
    Task<StatsOverviewDto> GetOverviewAsync(StatsOverviewQueryDto query, int? userId = null);
    Task<StatsTrendDto> GetTrendAsync(StatsTrendQueryDto query, int? userId = null);
    Task<List<MemberActivityStatsDto>> GetMemberActivityAsync(MemberActivityQueryDto query, int? userId = null);
}
