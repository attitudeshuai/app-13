using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.Domain.Interfaces;

public interface IExpiryAlertRepository : IRepository<ExpiryAlert, int>
{
    Task<PagedResult<ExpiryAlert>> GetByUserIdAsync(int userId, QueryParameters parameters, bool? isRead = null);
    Task<PagedResult<ExpiryAlert>> GetByFoodItemIdAsync(int foodItemId, QueryParameters parameters);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteByIdsAsync(IEnumerable<int> ids);
}
