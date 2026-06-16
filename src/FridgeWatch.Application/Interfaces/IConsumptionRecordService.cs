using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IConsumptionRecordService
{
    Task<PagedResultDto<ConsumptionRecordDto>> GetListAsync(QueryParametersDto parameters, int? foodItemId = null, int? householdId = null);
    Task<PagedResultDto<ConsumptionRecordDto>> GetMineAsync(int userId, QueryParametersDto parameters);
    Task<ConsumptionRecordDto> GetByIdAsync(int id);
    Task<ConsumptionRecordDto> CreateAsync(ConsumptionRecordCreateDto dto, int userId);
    Task<ConsumptionRecordDto> UpdateAsync(int id, ConsumptionRecordUpdateDto dto, int userId);
    Task DeleteAsync(int id, int userId);
}
