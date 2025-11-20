using Microsoft.AspNetCore.Mvc;
using Auth.Interfaces;
using Auth.Dto;


namespace Auth.Controllers;

[ApiController]
[Route("authentication")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthenticationController(IAuthService userService)
    {
        _authService = userService;
    }

    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResp), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup([FromBody] SignupReq req, CancellationToken ct)
    {
        try
        {
            var authResponse = await _authService.SignupAsync(req, ct);
            return Created(nameof(Login), authResponse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResp), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginReq req, CancellationToken ct)
    {
        var authResponse = await _authService.LoginAsync(req, ct);
        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        return Ok(authResponse);
    }
}
