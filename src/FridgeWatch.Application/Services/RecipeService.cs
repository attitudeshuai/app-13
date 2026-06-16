using AutoMapper;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<RecipeDto>> GetListAsync(QueryParametersDto parameters, string? category = null)
    {
        var queryParams = _mapper.Map<QueryParameters>(parameters);
        var recipes = await _unitOfWork.Recipes.GetPagedAsync(queryParams);

        var recipeDtos = new List<RecipeDto>();
        foreach (var recipe in recipes.Items)
        {
            var recipeWithIngredients = await _unitOfWork.Recipes.GetWithIngredientsAsync(recipe.Id);
            recipeDtos.Add(_mapper.Map<RecipeDto>(recipeWithIngredients));
        }

        return new PagedResultDto<RecipeDto>
        {
            Items = recipeDtos,
            TotalCount = recipes.TotalCount,
            PageNumber = recipes.PageNumber,
            PageSize = recipes.PageSize
        };
    }

    public async Task<RecipeDto> GetByIdAsync(int id)
    {
        var recipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(id);
        if (recipe == null)
        {
            throw new BusinessException("食谱不存在");
        }

        return _mapper.Map<RecipeDto>(recipe);
    }

    public async Task<RecipeDto> CreateAsync(RecipeCreateDto dto)
    {
        var recipe = _mapper.Map<Recipe>(dto);
        recipe.Ingredients = _mapper.Map<List<RecipeIngredient>>(dto.Ingredients);

        await _unitOfWork.Recipes.AddAsync(recipe);
        await _unitOfWork.SaveChangesAsync();

        var createdRecipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(recipe.Id);
        return _mapper.Map<RecipeDto>(createdRecipe);
    }

    public async Task<RecipeDto> UpdateAsync(int id, RecipeUpdateDto dto)
    {
        var recipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(id);
        if (recipe == null)
        {
            throw new BusinessException("食谱不存在");
        }

        _mapper.Map(dto, recipe);

        if (dto.Ingredients != null && dto.Ingredients.Count > 0)
        {
            recipe.Ingredients.Clear();
            foreach (var ingDto in dto.Ingredients)
            {
                var ingredient = _mapper.Map<RecipeIngredient>(ingDto);
                ingredient.RecipeId = id;
                recipe.Ingredients.Add(ingredient);
            }
        }

        await _unitOfWork.Recipes.UpdateAsync(recipe);
        await _unitOfWork.SaveChangesAsync();

        var updatedRecipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(id);
        return _mapper.Map<RecipeDto>(updatedRecipe);
    }

    public async Task DeleteAsync(int id)
    {
        var recipe = await _unitOfWork.Recipes.GetByIdAsync(id);
        if (recipe == null)
        {
            throw new BusinessException("食谱不存在");
        }

        await _unitOfWork.Recipes.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<RecipeRecommendationDto>> GetRecommendationsAsync(RecipeRecommendRequestDto request, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(request.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法查看推荐");
        }

        var availableFoodItems = await GetAvailableFoodItemsForRecommendations(request);
        if (availableFoodItems.Count == 0)
        {
            return new List<RecipeRecommendationDto>();
        }

        var allRecipes = await _unitOfWork.Recipes.GetAllWithIngredientsAsync();
        if (allRecipes.Count == 0)
        {
            return new List<RecipeRecommendationDto>();
        }

        var recommendations = new List<RecipeRecommendationDto>();

        foreach (var recipe in allRecipes)
        {
            var recommendation = CalculateRecipeMatch(recipe, availableFoodItems);
            recommendations.Add(recommendation);
        }

        return recommendations
            .OrderByDescending(r => r.MatchScore)
            .ThenByDescending(r => r.NearExpiryUsedCount + r.ExpiredUsedCount)
            .Take(request.MaxResults)
            .ToList();
    }

    private async Task<List<FoodItem>> GetAvailableFoodItemsForRecommendations(RecipeRecommendRequestDto request)
    {
        var today = DateTime.UtcNow.Date;
        var maxExpiredDate = request.IncludeExpired && request.MaxExpiredDays.HasValue
            ? today.AddDays(-request.MaxExpiredDays.Value)
            : today;

        var allFoodItems = await _unitOfWork.FoodItems
            .FindAsync(f => f.HouseholdId == request.HouseholdId &&
                            f.Status != FoodStatus.Consumed &&
                            f.Quantity > 0);

        var filteredItems = allFoodItems
            .Where(f =>
            {
                var daysToExpiry = (f.ExpiryDate.Date - today).Days;
                if (daysToExpiry >= 0 && daysToExpiry <= 3)
                {
                    return true;
                }
                if (request.IncludeExpired && daysToExpiry < 0 && f.ExpiryDate.Date >= maxExpiredDate)
                {
                    return true;
                }
                return false;
            })
            .OrderBy(f => f.ExpiryDate)
            .ToList();

        return filteredItems;
    }

    private RecipeRecommendationDto CalculateRecipeMatch(Recipe recipe, List<FoodItem> availableFoodItems)
    {
        var recommendation = new RecipeRecommendationDto
        {
            Recipe = _mapper.Map<RecipeDto>(recipe),
            MatchingIngredients = new List<MatchingIngredientDto>(),
            MissingIngredients = new List<string>()
        };

        var today = DateTime.UtcNow.Date;

        foreach (var recipeIngredient in recipe.Ingredients)
        {
            var matchedFoodItem = FindMatchingFoodItem(recipeIngredient, availableFoodItems);

            if (matchedFoodItem != null)
            {
                var daysToExpiry = (matchedFoodItem.ExpiryDate.Date - today).Days;
                var isNearExpiry = daysToExpiry >= 0 && daysToExpiry <= 3;
                var isExpired = daysToExpiry < 0;

                var matchingIngredient = new MatchingIngredientDto
                {
                    IngredientName = recipeIngredient.IngredientName,
                    RequiredQuantity = recipeIngredient.Quantity,
                    RequiredUnit = recipeIngredient.Unit,
                    AvailableQuantity = matchedFoodItem.Quantity,
                    AvailableUnit = matchedFoodItem.Unit,
                    IsNearExpiry = isNearExpiry,
                    IsExpired = isExpired,
                    FoodItemId = matchedFoodItem.Id,
                    FoodItemName = matchedFoodItem.Name
                };

                recommendation.MatchingIngredients.Add(matchingIngredient);

                if (isNearExpiry)
                {
                    recommendation.NearExpiryUsedCount++;
                    recommendation.MatchScore += 30;
                }
                else if (isExpired)
                {
                    recommendation.ExpiredUsedCount++;
                    recommendation.MatchScore += 15;
                }
                else
                {
                    recommendation.MatchScore += 10;
                }
            }
            else
            {
                if (!recipeIngredient.IsOptional)
                {
                    recommendation.MissingIngredients.Add($"{recipeIngredient.IngredientName} ({recipeIngredient.Quantity}{recipeIngredient.Unit})");
                    recommendation.MatchScore -= 5;
                }
            }
        }

        var totalIngredients = recipe.Ingredients.Count(i => !i.IsOptional);
        var matchedCount = recipe.Ingredients.Count(i => !i.IsOptional &&
            FindMatchingFoodItem(i, availableFoodItems) != null);

        if (totalIngredients > 0)
        {
            var matchPercentage = (double)matchedCount / totalIngredients * 100;
            recommendation.MatchScore += (int)matchPercentage;
        }

        if (recommendation.MatchScore < 0)
        {
            recommendation.MatchScore = 0;
        }

        return recommendation;
    }

    private static FoodItem? FindMatchingFoodItem(RecipeIngredient recipeIngredient, List<FoodItem> foodItems)
    {
        var ingredientName = recipeIngredient.IngredientName.Trim();
        var ingredientCategory = recipeIngredient.IngredientCategory.Trim();

        var exactMatch = foodItems.FirstOrDefault(f =>
            f.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase) ||
            f.Name.Contains(ingredientName, StringComparison.OrdinalIgnoreCase) ||
            ingredientName.Contains(f.Name, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        var categoryMatch = foodItems.FirstOrDefault(f =>
            f.Category.Equals(ingredientCategory, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(ingredientCategory));

        return categoryMatch;
    }
}
