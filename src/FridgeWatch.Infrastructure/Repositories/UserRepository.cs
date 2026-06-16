using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Repositories;

public class UserRepository : Repository<User, int>, IUserRepository
{
    public UserRepository(FridgeWatchDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbSet.FirstOrDefaultAsync(u =>
            u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    protected override IQueryable<User> ApplySearch(IQueryable<User> query, string searchTerm)
    {
        return query.Where(u =>
            u.Username.Contains(searchTerm) ||
            u.Email.Contains(searchTerm));
    }
}
