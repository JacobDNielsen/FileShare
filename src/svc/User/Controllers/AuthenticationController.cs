using Microsoft.AspNetCore.Mvc;
using User.Interfaces;
using User.Dto;


namespace User.Controllers;

[ApiController]
[Route("authentication")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResp), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup([FromBody] SignupReq req, CancellationToken ct)
    {
        try
        {
            var authResponse = await _userService.SignupAsync(req, ct);
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
        var authResponse = await _userService.LoginAsync(req, ct);
        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        return Ok(authResponse);
    }
}
