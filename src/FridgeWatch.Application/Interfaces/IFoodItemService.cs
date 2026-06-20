using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.Interfaces;

public interface IFoodItemService
{
    Task<PagedResultDto<FoodItemDto>> GetListAsync(FoodItemQueryParametersDto parameters, int? householdId = null, int? userId = null);
    Task<FoodItemDetailDto> GetByIdAsync(int id);
    Task<FoodItemDto> CreateAsync(FoodItemCreateDto dto, int userId, Stream? photoStream = null, string? photoFileName = null);
    Task<FoodItemDto> UpdateAsync(int id, FoodItemUpdateDto dto, int userId, Stream? photoStream = null, string? photoFileName = null);
    Task DeleteAsync(int id, int userId);
    Task<FoodItemDto> UpdateStatusAsync(int id, FoodStatus status, int userId);
    Task<FoodItemImportResultDto> BulkImportAsync(int householdId, Stream fileStream, string fileName, int userId);
    Task<byte[]> DownloadTemplateAsync();
}
