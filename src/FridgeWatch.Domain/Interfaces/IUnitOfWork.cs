namespace FridgeWatch.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IHouseholdRepository Households { get; }
    IHouseholdMemberRepository HouseholdMembers { get; }
    IFoodItemRepository FoodItems { get; }
    IExpiryAlertRepository ExpiryAlerts { get; }
    IConsumptionRecordRepository ConsumptionRecords { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
