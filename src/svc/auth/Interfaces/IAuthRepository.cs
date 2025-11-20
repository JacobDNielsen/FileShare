using Auth.Models;

namespace Auth.Interfaces;

public interface IAuthRepository
{
    Task<bool> ExistsByUsernameOrEmailAsync(string userName, string email, CancellationToken ct);
    Task<UserAccount?> GetUserByUsernameAsync(string userName, CancellationToken ct);
    Task AddUserAsync(UserAccount user, CancellationToken ct);
    Task UpdateUserAsync(UserAccount user, CancellationToken ct);
}