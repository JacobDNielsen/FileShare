namespace User.Models;

public sealed record JwtConfig
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
    public int ExpiresMinutes { get; init; } = 120;
}