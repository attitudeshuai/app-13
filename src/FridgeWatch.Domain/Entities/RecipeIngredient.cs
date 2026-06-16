using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Entities;

public class RecipeIngredient : BaseEntity<int>
{
    public int RecipeId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string IngredientCategory { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOptional { get; set; }

    public virtual Recipe? Recipe { get; set; }
}
