using User.Models;

namespace User.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByUsernameOrEmailAsync(string userName, string email, CancellationToken ct);
    Task<UserAccount?> GetUserByUsernameAsync(string userName, CancellationToken ct);
    Task AddUserAsync(UserAccount user, CancellationToken ct);
    Task UpdateUserAsync(UserAccount user, CancellationToken ct);
}