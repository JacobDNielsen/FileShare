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

    public UserService(WopiDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct)
    {
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
            Email = email,
            PasswordHash = req.Password, // TODO: Hash password
        };

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
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName, ct);

        if (user == null || user.PasswordHash != password) // TODO: Verify hashed password
        {
            return null;
        }

        var token = _jwtService.JwtTokenGenerator(user.Id.ToString(), user.UserName);
        return new AuthResp(user.UserName, "Bearer", token);
    }

}