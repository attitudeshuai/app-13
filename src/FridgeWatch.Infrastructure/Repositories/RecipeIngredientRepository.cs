using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class RecipeIngredientRepository : Repository<RecipeIngredient, int>, IRecipeIngredientRepository
{
    public RecipeIngredientRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<List<RecipeIngredient>> GetByRecipeIdAsync(int recipeId)
    {
        return await _dbSet
            .Where(ri => ri.RecipeId == recipeId)
            .ToListAsync();
    }
}
