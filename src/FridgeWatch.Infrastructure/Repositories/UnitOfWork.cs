using Microsoft.EntityFrameworkCore.Storage;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;
using FridgeWatch.Infrastructure.Repositories;

namespace FridgeWatch.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FridgeWatchDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; private set; }
    public IHouseholdRepository Households { get; private set; }
    public IHouseholdMemberRepository HouseholdMembers { get; private set; }
    public IFoodItemRepository FoodItems { get; private set; }
    public IExpiryAlertRepository ExpiryAlerts { get; private set; }
    public IConsumptionRecordRepository ConsumptionRecords { get; private set; }
    public IShoppingListRepository ShoppingLists { get; private set; }
    public IShoppingListItemRepository ShoppingListItems { get; private set; }
    public IShareLinkRepository ShareLinks { get; private set; }
    public IRecipeRepository Recipes { get; private set; }
    public IRecipeIngredientRepository RecipeIngredients { get; private set; }

    public UnitOfWork(FridgeWatchDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Households = new HouseholdRepository(context);
        HouseholdMembers = new HouseholdMemberRepository(context);
        FoodItems = new FoodItemRepository(context);
        ExpiryAlerts = new ExpiryAlertRepository(context);
        ConsumptionRecords = new ConsumptionRecordRepository(context);
        ShoppingLists = new ShoppingListRepository(context);
        ShoppingListItems = new ShoppingListItemRepository(context);
        ShareLinks = new ShareLinkRepository(context);
        Recipes = new RecipeRepository(context);
        RecipeIngredients = new RecipeIngredientRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
