using Microsoft.AspNetCore.Mvc;

namespace Storage.Controllers;

[ApiController]
[Route("benchmark")]
public sealed class BenchmarkController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong" });
    }
}