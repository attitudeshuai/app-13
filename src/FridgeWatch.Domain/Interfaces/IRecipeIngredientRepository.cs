using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Domain.Interfaces;

public interface IRecipeIngredientRepository : IRepository<RecipeIngredient, int>
{
    Task<List<RecipeIngredient>> GetByRecipeIdAsync(int recipeId);
}
