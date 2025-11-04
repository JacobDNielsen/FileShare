
using Microsoft.AspNetCore.Identity;
using User.Dto;
using User.Models;
using User.Interfaces;

namespace User.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public UserService(IUserRepository repository, IJwtService jwtService, IPasswordHasher<UserAccount> passwordHasher)
    {
        _repository = repository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;

    }

    public async Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            throw new InvalidOperationException("Username, email, and password are required");
        }
        var userName = req.UserName.Trim().ToLowerInvariant();
        var email = req.Email.Trim().ToLowerInvariant();
        var password = req.Password;

        if (await _repository.ExistsByUsernameOrEmailAsync(userName, email, ct))
        {
            throw new InvalidOperationException("User with that username or email already exists");
        }

        var user = new UserAccount
        {
            UserName = userName,
            Email = email
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        await _repository.AddUserAsync(user, ct);

        var token = _jwtService.JwtTokenGenerator(user.Id.ToString(), user.UserName); 
        return new AuthResp(user.UserName, "Bearer", token);
    }

    public async Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct)
    {
        var userName = req.UserName.Trim().ToLowerInvariant();
        var password = req.Password;

        var user = await _repository.GetUserByUsernameAsync(userName, ct);
        if (user == null)
        {
            return null;
        }
        var isCorrectHash = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (isCorrectHash == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (isCorrectHash == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _repository.UpdateUserAsync(user, ct);

        var token = _jwtService.JwtTokenGenerator(user.Id.ToString(), user.UserName);
        return new AuthResp(user.UserName, "Bearer", token);
    }

}