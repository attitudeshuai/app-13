using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class RecipeRepository : Repository<Recipe, int>, IRecipeRepository
{
    public RecipeRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<Recipe?> GetWithIngredientsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Recipe>> GetAllWithIngredientsAsync()
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .ToListAsync();
    }

    public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm, string? category = null)
    {
        var query = _dbSet.Include(r => r.Ingredients).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r =>
                r.Name.Contains(searchTerm) ||
                r.Description.Contains(searchTerm) ||
                r.Category.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.Category == category);
        }

        return await query.ToListAsync();
    }

    protected override IQueryable<Recipe> ApplySearch(IQueryable<Recipe> query, string searchTerm)
    {
        return query.Where(r =>
            r.Name.Contains(searchTerm) ||
            r.Description.Contains(searchTerm) ||
            r.Category.Contains(searchTerm));
    }

    protected override IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "cooktime" => descending ? query.OrderByDescending(r => r.CookTimeMinutes) : query.OrderBy(r => r.CookTimeMinutes),
            "difficulty" => descending ? query.OrderByDescending(r => r.Difficulty) : query.OrderBy(r => r.Difficulty),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };
    }
}
