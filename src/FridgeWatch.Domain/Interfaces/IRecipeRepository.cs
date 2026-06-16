using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Domain.Interfaces;

public interface IRecipeRepository : IRepository<Recipe, int>
{
    Task<Recipe?> GetWithIngredientsAsync(int id);
    Task<List<Recipe>> GetAllWithIngredientsAsync();
    Task<List<Recipe>> SearchRecipesAsync(string searchTerm, string? category = null);
}
