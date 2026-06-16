namespace FridgeWatch.Application.DTOs;

public class RecipeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int CookTimeMinutes { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Servings { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
}

public class RecipeIngredientDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string IngredientCategory { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

public class RecipeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int CookTimeMinutes { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Servings { get; set; }
    public List<RecipeIngredientCreateDto> Ingredients { get; set; } = new();
}

public class RecipeIngredientCreateDto
{
    public string IngredientName { get; set; } = string.Empty;
    public string IngredientCategory { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

public class RecipeUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int? Difficulty { get; set; }
    public int? CookTimeMinutes { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public int? Servings { get; set; }
    public List<RecipeIngredientUpdateDto>? Ingredients { get; set; }
}

public class RecipeIngredientUpdateDto
{
    public int? Id { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string IngredientCategory { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

public class RecipeRecommendationDto
{
    public RecipeDto Recipe { get; set; } = new();
    public int MatchScore { get; set; }
    public List<MatchingIngredientDto> MatchingIngredients { get; set; } = new();
    public List<string> MissingIngredients { get; set; } = new();
    public int NearExpiryUsedCount { get; set; }
    public int ExpiredUsedCount { get; set; }
}

public class MatchingIngredientDto
{
    public string IngredientName { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public string RequiredUnit { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }
    public string AvailableUnit { get; set; } = string.Empty;
    public bool IsNearExpiry { get; set; }
    public bool IsExpired { get; set; }
    public int? FoodItemId { get; set; }
    public string FoodItemName { get; set; } = string.Empty;
}

public class RecipeRecommendRequestDto
{
    public int HouseholdId { get; set; }
    public int MaxResults { get; set; } = 5;
    public bool IncludeExpired { get; set; } = false;
    public int? MaxExpiredDays { get; set; } = 3;
}
