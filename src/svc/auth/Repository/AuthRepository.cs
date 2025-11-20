using Auth.Interfaces;
using Auth.Models;
using Auth.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _dbContext;

    public AuthRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByUsernameOrEmailAsync(string userName, string email, CancellationToken ct)
    {
        return await _dbContext.UserAccounts
        .AsNoTracking()
        .AnyAsync(u => u.UserName == userName || u.Email == email, ct);
    }

    public async Task<UserAccount?> GetUserByUsernameAsync(string userName, CancellationToken ct)
    {
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.UserName == userName, ct);
    }

    public async Task AddUserAsync(UserAccount user, CancellationToken ct)
    {
        await _dbContext.UserAccounts.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateUserAsync(UserAccount user, CancellationToken ct)
    {
        _dbContext.UserAccounts.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }
}