using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using User.Services;
using User.Models;

[ApiController]
[Route(".well-known")]
public sealed class JwksController : ControllerBase
{
    [HttpGet]
    [Route("jwks.json")]
    public IActionResult Get([FromServices] JwtSigningKeyStore keyStore)
    {
        Response.Headers["Cache-Control"] = "public, max-age=7000, must-revalidate"; //Expires after 116,6 minutes, after which clients should revalidate
        return Ok(new { keys = keyStore.GetAllPublicJwks() });
    }

    [HttpGet("openid-configuration")]
    public IActionResult OpenIdConfig([FromServices] IOptions<JwtConfig> configuration)
    {
        var issuer = configuration.Value.Issuer.TrimEnd('/');

        return Ok(new
        {
            issuer,
            jwks_uri = $"{issuer}/.well-known/jwks.json",
            id_token_signing_alg_values_supported = new[] { SecurityAlgorithms.RsaSha256 }
        });
    }
}
