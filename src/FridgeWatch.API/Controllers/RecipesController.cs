using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.API.Controllers;

[Route("api/recipes")]
[Authorize]
public class RecipesController : ApiControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QueryParametersDto parameters,
        [FromQuery] string? category = null)
    {
        var result = await _recipeService.GetListAsync(parameters, category);
        return Success(result, "获取成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _recipeService.GetByIdAsync(id);
        return Success(result, "获取成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RecipeCreateDto dto)
    {
        var result = await _recipeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RecipeDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RecipeUpdateDto dto)
    {
        var result = await _recipeService.UpdateAsync(id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _recipeService.DeleteAsync(id);
        return Success("删除成功");
    }

    [HttpPost("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromBody] RecipeRecommendRequestDto request)
    {
        var userId = GetCurrentUserId();
        var result = await _recipeService.GetRecommendationsAsync(request, userId);
        return Success(result, "获取推荐成功");
    }

    [HttpGet("recommend")]
    public async Task<IActionResult> GetRecommendationsByQuery(
        [FromQuery] int householdId,
        [FromQuery] int maxResults = 5,
        [FromQuery] bool includeExpired = false,
        [FromQuery] int maxExpiredDays = 3)
    {
        var userId = GetCurrentUserId();
        var request = new RecipeRecommendRequestDto
        {
            HouseholdId = householdId,
            MaxResults = maxResults,
            IncludeExpired = includeExpired,
            MaxExpiredDays = maxExpiredDays
        };
        var result = await _recipeService.GetRecommendationsAsync(request, userId);
        return Success(result, "获取推荐成功");
    }
}
