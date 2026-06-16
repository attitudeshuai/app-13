using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Domain.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
}
