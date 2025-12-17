using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WopiHost.Dto;

[ApiController]
[Authorize]
[Route("wopi/files")]
public sealed class WopiFilesController : ControllerBase
{
    private readonly IStorageClient _storage;
    private readonly ILockClient _lock;
    public WopiFilesController(IStorageClient storage, ILockClient client)
    { 
        _storage = storage;
        _lock = client;
    }

    // WOPI: CheckFileInfo proxy
    [HttpGet]
    public async Task<IActionResult> CheckFileInfo(string id, CancellationToken ct)
    {
        var info = await _storage.CheckFileInfoAsync(id, ct);
        return info is null ? NotFound() : Ok(info);
    }

    // WOPI: GetFile (contents)
    [HttpGet("{file_id}/contents")]
    public async Task<IActionResult> GetFile(string file_id, CancellationToken ct)
    {
        var stream = await _storage.GetFile(file_id, ct);
        return File(stream, "application/octet-stream");
    }

    [HttpPost("{file_id}")]
    public async Task<IActionResult> Post(
        string file_id,
        [FromHeader(Name = "X-WOPI-Override")] string overrideVerb, 
        [FromHeader(Name = "X-WOPI-Lock")] string wopiLock,
        CancellationToken ct)
    {
        return overrideVerb switch
        {
            "GET_LOCK" => await GetLock(file_id, ct),
            "LOCK" => await Lock(file_id, wopiLock, ct),
            "REFRESH_LOCK" => await RefreshLock(file_id, wopiLock, ct),
            "UNLOCK" => await Unlock(file_id, wopiLock, ct),

            _ => BadRequest("Unknown WOPI operation")

        };
    }

    private async Task<IActionResult> GetLock(string file_id, CancellationToken ct)
    {
        var response = await _lock.GetLockAsync(file_id, ct);

        Response.Headers["X-WOPI-Lock"] = response.Headers.Contains("X-WOPI-Lock")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-Lock"))
            : string.Empty;

        return Ok();
    }

    private async Task<IActionResult> Lock(string file_id, string lockValue, CancellationToken ct)
    {
        var response = await _lock.LockAsync(file_id, lockValue, ct);

        if (response.IsSuccessStatusCode)
        {
            Response.Headers["X-WOPI-Lock"] = lockValue;
            return Ok();
        }

        Response.Headers["X-WOPI-Lock"] = response.Headers.Contains("X-WOPI-Lock")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-Lock"))
            : string.Empty;

        Response.Headers["X-WOPI-LockFailureReason"] = response.Headers.Contains("X-WOPI-LockFailureReason")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-LockFailureReason"))
            : "Lock conflict";

        return Conflict();
    }

    private async Task<IActionResult> RefreshLock(string file_id, string lockValue, CancellationToken ct)
    {
        var response = await _lock.RefreshLockAsync(file_id, lockValue, ct);

        if (response.IsSuccessStatusCode)
        {
            Response.Headers["X-WOPI-Lock"] = lockValue;
            return Ok();
        }

        Response.Headers["X-WOPI-Lock"] = response.Headers.Contains("X-WOPI-Lock")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-Lock"))
            : string.Empty;

        Response.Headers["X-WOPI-LockFailureReason"] = response.Headers.Contains("X-WOPI-LockFailureReason")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-LockFailureReason"))
            : "Lock conflict";

        return Conflict();
    }

    private async Task<IActionResult> Unlock(string file_id, string lockValue, CancellationToken ct)
    {
        var response = await _lock.UnlockAsync(file_id, lockValue, ct);

        if (response.IsSuccessStatusCode)
            return Ok();

        Response.Headers["X-WOPI-Lock"] = response.Headers.Contains("X-WOPI-Lock")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-Lock"))
            : string.Empty;

        Response.Headers["X-WOPI-LockFailureReason"] = response.Headers.Contains("X-WOPI-LockFailureReason")
            ? string.Join(",", response.Headers.GetValues("X-WOPI-LockFailureReason"))
            : "Lock conflict";

        return Conflict();
    }


[HttpGet("{id}/urlBuilder")]
public async Task<IActionResult> UrlBuilder([FromRoute] string id)
    {
        var url = $"http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/{id}&access_token=securetoken";
        return Ok(url);
    }
}
