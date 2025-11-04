namespace User.Interfaces;

public interface IJwtService
{
    string JwtTokenGenerator(string userId, string userName);
}