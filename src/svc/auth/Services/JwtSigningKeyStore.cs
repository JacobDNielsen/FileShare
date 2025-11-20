using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Auth.Models;

namespace Auth.Services;

public sealed class JwtSigningKeyStore
{
    private readonly Dictionary<string, RsaSecurityKey> _signingKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, JsonWebKey> _publicJwks = new(StringComparer.Ordinal);

    public string LatestKeyId { get; }

    public JwtSigningKeyStore(IOptions<JwtConfig> options)
    {
        var config = options.Value;

        if (string.IsNullOrWhiteSpace(config.LatestKeyId))
        {
            throw new InvalidOperationException("Authentication:Jwt:LatestKeyId is missing!");
        }

        if (config.SigningKeys is null || config.SigningKeys.Count == 0 )
        {
            throw new InvalidOperationException("Authentication:Jwt:SigningKeys is missing or empty!");
        }

        foreach (var signingKey in config.SigningKeys)
        {
            // We simply skip invalid keys, instead of crashing. Good if we fx. don't have the private key for older keys, or if adding new public key but not yet have added the private key in PEM
            if (string.IsNullOrWhiteSpace(signingKey.KeyId))
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(signingKey.PrivateKey))
            {
                continue;
            }

            var rsa = RSA.Create();
            rsa.ImportFromPem(signingKey.PrivateKey.AsSpan()); //InportFromPem uses ReadOnlySpan, we thus convert the string to Span (a readonly, non-allocating view of a string. Aka less memory allocation)

            var rsaKey = new RsaSecurityKey(rsa) //Creates RSA key from the imported PEM, including both public and private parts
            {
                KeyId = signingKey.KeyId
            };
            _signingKeys[signingKey.KeyId] = rsaKey;

            var publicParameter = rsa.ExportParameters(false); //We dont explude the private parameters, only the public ones
            _publicJwks[signingKey.KeyId] = new JsonWebKey
            {
                Kid = signingKey.KeyId,
                Kty = "RSA",
                Use = "sig",
                E = Base64UrlEncoder.Encode(publicParameter.Exponent!),
                N = Base64UrlEncoder.Encode(publicParameter.Modulus!),
                Alg = SecurityAlgorithms.RsaSha256
            };
        }

            if (!_signingKeys.ContainsKey(config.LatestKeyId))
            {
                throw new InvalidOperationException($"LatestKeyId '{config.LatestKeyId}' does not have a corresponding signing key, ensure that the PrivateKey is set!");
            }

            LatestKeyId = config.LatestKeyId;
        }
    

    public SigningCredentials GetLatestSigningCredentials()
    {
        var rsaKey = _signingKeys[LatestKeyId];
        return new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
    }

    public IEnumerable<JsonWebKey> GetAllPublicJwks()
    {
        return _publicJwks.Values;
    }

}
