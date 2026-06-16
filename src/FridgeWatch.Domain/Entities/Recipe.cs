using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class Recipe : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int CookTimeMinutes { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Servings { get; set; }

    public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
}
