#if DEBUG
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("debug")]
public class DebugController : ControllerBase
{
    [HttpPost("decode-token")]
    public IActionResult DecodeToken([FromBody] string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        var header = jwt.Header;
        var kid = header["kid"]?.ToString() ?? "(none)"; 

        return Ok(new
        {
            Header = header,
            Payload = jwt.Payload,
            kid
        });
    }
}
# endif