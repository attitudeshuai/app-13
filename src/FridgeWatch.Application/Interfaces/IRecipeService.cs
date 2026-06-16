using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Application.Interfaces;

public interface IRecipeService
{
    Task<PagedResultDto<RecipeDto>> GetListAsync(QueryParametersDto parameters, string? category = null);
    Task<RecipeDto> GetByIdAsync(int id);
    Task<RecipeDto> CreateAsync(RecipeCreateDto dto);
    Task<RecipeDto> UpdateAsync(int id, RecipeUpdateDto dto);
    Task DeleteAsync(int id);
    Task<List<RecipeRecommendationDto>> GetRecommendationsAsync(RecipeRecommendRequestDto request, int userId);
}
