using Microsoft.AspNetCore.Mvc;
using User.Services;

[ApiController]
public sealed class JwksController : ControllerBase
{
    [HttpGet]
    [Route("/.well-known/jwks.json")]
    public IActionResult Get([FromServices] JwtSigningKeyStore keyStore)
    {
        Response.Headers["Cache-Control"] = "public, max-age=7000, must-revalidate"; //Expires after 116,6 minutes, after which clients should revalidate
        return Ok(new {keys = keyStore.GetAllPublicJwks()});
    }
}
