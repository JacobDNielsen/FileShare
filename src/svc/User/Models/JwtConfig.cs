namespace User.Models;

/// <summary>
///  Our JWT configuration settings, used for issuing JWT access tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description><see cref="Issuer"/> represents the tokens <c>iss</c> claim.</description></item>
/// <item><description><see cref="Audience"/> represents the tokens <c>aud</c> claim.</description></item>
/// <item><description><see cref="LatestKeyId"/> is the latest key that is used for signing tokens</description></item>
/// <item><description><see cref="SigningKeys"/> contains both current and potentially older keys, in case we want to validate older keys if rotating</description></item>
/// </list>
/// </remarks>
public sealed class JwtConfig
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiresMinutes { get; set; } = 120;
    public string LatestKeyId { get; set; } = default!;
    public List<JwtSigningKey> SigningKeys { get; set; } = new();
}

public sealed class JwtSigningKey
{
    public string KeyId { get; set; } = default!;
    public string? PrivateKey { get; set; }
}