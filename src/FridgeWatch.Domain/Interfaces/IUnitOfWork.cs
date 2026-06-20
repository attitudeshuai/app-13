namespace FridgeWatch.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IHouseholdRepository Households { get; }
    IHouseholdMemberRepository HouseholdMembers { get; }
    IFoodItemRepository FoodItems { get; }
    IExpiryAlertRepository ExpiryAlerts { get; }
    IConsumptionRecordRepository ConsumptionRecords { get; }
    IShoppingListRepository ShoppingLists { get; }
    IShoppingListItemRepository ShoppingListItems { get; }
    IShareLinkRepository ShareLinks { get; }
    IRecipeRepository Recipes { get; }
    IRecipeIngredientRepository RecipeIngredients { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
