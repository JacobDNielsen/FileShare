namespace WopiHost.Services;

public interface IJwtService
{
    string JwtTokenGenerator(string userId, string userName);
}