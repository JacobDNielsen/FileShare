using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WopiHost.Data;
using WopiHost.Dto;
using WopiHost.Models;

namespace WopiHost.Services;

public class UserService : IUserService
{
    private readonly WopiDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public UserService(WopiDbContext dbContext, IJwtService jwtService, IPasswordHasher<UserAccount> passwordHasher)
    {
        _dbContext = dbContext;
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

        var doesUserExists = await _dbContext.UserAccounts
        .AsNoTracking()
        .AnyAsync(u => u.UserName == userName || u.Email == email, ct);

        if (doesUserExists)
        {
            throw new InvalidOperationException("User with that username or email already exists");
        }

        var user = new UserAccount
        {
            UserName = userName,
            Email = email
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _dbContext.UserAccounts.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        var token = _jwtService.JwtTokenGenerator(user.Id.ToString(), user.UserName);
        return new AuthResp(user.UserName, "Bearer", token);
    }

    public async Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct)
    {
        var userName = req.UserName.Trim().ToLowerInvariant();
        var password = req.Password;

        var user = await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.UserName == userName, ct);

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
        await _dbContext.SaveChangesAsync(ct);

        var token = _jwtService.JwtTokenGenerator(user.Id.ToString(), user.UserName);
        return new AuthResp(user.UserName, "Bearer", token);
    }

}