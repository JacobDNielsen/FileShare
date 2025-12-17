using Microsoft.AspNetCore.Mvc;
using Lock.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
namespace Lock.Controllers;

[ApiController]
[Route("wopi/locks")]
public class LockController : ControllerBase
{
    private readonly ILockManager _lockManager;

    public LockController(ILockManager lockManager)
    {
        _lockManager = lockManager;
    }

    [HttpPost("{file_id}")]
    public async Task<IActionResult> Lock([FromRoute] string file_id, CancellationToken ct)
    {
        var requestedLock = Request.Headers["X-WOPI-Lock"].ToString();

        if (string.IsNullOrWhiteSpace(requestedLock))
            return BadRequest("Missing X-WOPI-Lock header");

        var result = await _lockManager.LockAsync(file_id, requestedLock, ct);

        if (!result.Success)
        {
            Response.Headers["X-WOPI-Lock"] = result.ExistingLock ?? string.Empty;
            Response.Headers["X-WOPI-LockFailureReason"] = result.Reason ?? "Lock mismatch.";
            return StatusCode(StatusCodes.Status409Conflict);
        }

        Response.Headers["X-WOPI-Lock"] = requestedLock;
        return Ok();
    }

    [HttpPost("{file_id}/refresh")]
    public async Task<IActionResult> Refresh([FromRoute] string file_id, CancellationToken ct)
    {
        var requestedLock = Request.Headers["X-WOPI-Lock"].ToString();
        if (string.IsNullOrWhiteSpace(requestedLock))
            return BadRequest("Missing X-WOPI-Lock header");

        var result = await _lockManager.RefreshLockAsync(file_id, requestedLock, ct);

        if (!result.Success)
        {
            Response.Headers["X-WOPI-Lock"] = result.ExistingLock ?? string.Empty;
            Response.Headers["X-WOPI-LockFailureReason"] = result.Reason ?? "Lock mismatch.";
            return StatusCode(StatusCodes.Status409Conflict);
        }

        Response.Headers["X-WOPI-Lock"] = requestedLock;
        return Ok();
    }

    [HttpDelete("{file_id}")]
    public async Task<IActionResult> Unlock([FromRoute] string file_id, CancellationToken ct)
    {
        var requestedLock = Request.Headers["X-WOPI-Lock"].ToString();
        if (string.IsNullOrWhiteSpace(requestedLock))
            return BadRequest("Missing X-WOPI-Lock header");

        var result = await _lockManager.UnlockAsync(file_id, requestedLock, ct);

        if (!result.Success)
        {
            Response.Headers["X-WOPI-Lock"] = result.ExistingLock ?? string.Empty;
            Response.Headers["X-WOPI-LockFailureReason"] = result.Reason ?? "Lock mismatch.";
            return StatusCode(StatusCodes.Status409Conflict);
        }

        return Ok();
    }

    [HttpGet("{file_id}")]
    public async Task<IActionResult> GetLock([FromRoute] string file_id, CancellationToken ct)
    {
        var existing = await _lockManager.GetLockAsync(file_id, ct);
        Response.Headers["X-WOPI-Lock"] = existing?.LockId ?? string.Empty;
        return Ok();
    }
}