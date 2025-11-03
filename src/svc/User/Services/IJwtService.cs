namespace User.Services;

public interface IJwtService
{
    string JwtTokenGenerator(string userId, string userName);
}