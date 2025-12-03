using Microsoft.AspNetCore.Mvc;
using WopiHost.Dto;

[ApiController]
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
            "REFRESH_LOCk" => await RefreshLock(file_id, wopiLock, ct),
            "UNLOCK" => await Unlock(file_id, wopiLock, ct),

            _ => BadRequest("Unknown WOPI operation")

        };
    }

       private async Task<IActionResult> GetLock(string file_id, CancellationToken ct)
    {
        var result = await _lock.GetLockAsync(file_id, ct);
       
        //even if the lock is null status code 200 should still be returned
        Response.Headers["X-WOPI-Lock"] = result.LockValue ?? string.Empty;

        return Ok();
    }

    private async Task<IActionResult> Lock(string file_id, string newLock, CancellationToken ct)
    {
        var existingLock = await _lock.GetLockAsync(file_id, ct);
        var existingVaule = existingLock?.LockValue;

        if(existingLock == null)
        {
            
            var result = await _lock.SetLockAsync(file_id, new LockRequest{LockValue = newLock}, ct);
            if(result.Success)
                return Ok();

            //race: fetch existing and return conflict
            Response.Headers["X-WOPI-LOCK"] = result.ExistingLock ?? string.Empty;
            return Conflict();
            
        }

        if(existingVaule != newLock){
            Response.Headers["X-WOPI-LOCK"] = existingVaule;
            return Conflict();
        }

        return Ok();
    }

    private async Task<IActionResult> RefreshLock(string file_id, string newLock, CancellationToken ct)
    {
        var existingLock = await _lock.GetLockAsync(file_id, ct);
        var existingValue = existingLock?.LockValue;

    if (existingLock == null)
    {
        Response.Headers["X-WOPI-Lock"] = string.Empty;
        return Conflict();
    }

   
    if (existingValue != newLock)
    {
        Response.Headers["X-WOPI-Lock"] = existingValue ?? string.Empty;
        return Conflict();
    }


    var result = await _lock.RefreshLockAsync(file_id,
        new LockRequest { LockValue = newLock }, ct);

    if (result.Success)
    {
       
        Response.Headers["X-WOPI-Lock"] = newLock;
        return Ok();
    }

    var raceExisting = await _lock.GetLockAsync(file_id, ct);
    Response.Headers["X-WOPI-Lock"] = raceExisting?.LockValue ?? string.Empty;
    return Conflict();
}

    

    private async Task<IActionResult> Unlock(string file_id, string file_lock, CancellationToken ct)
    {
       var existingLock = await _lock.GetLockAsync(file_id, ct);
        var existingValue = existingLock?.LockValue; 

            // CASE 1: No lock exists -> Unlock must fail
    if (existingLock == null)
    {
        Response.Headers["X-WOPI-Lock"] = string.Empty;
        return Conflict();
    }

   
    if (existingValue != file_lock)
    {
        Response.Headers["X-WOPI-Lock"] = existingValue ?? string.Empty;
        return Conflict();
    }

   
    var result = await _lock.UnlockAsync(
        file_id,
        new LockRequest { LockValue = file_lock },
        ct);

    if (result.Success)
    {
        return Ok();
    }


    var raceExisting = await _lock.GetLockAsync(file_id, ct);
    Response.Headers["X-WOPI-Lock"] = raceExisting?.LockValue ?? string.Empty;

    return Conflict();
}

    

[HttpGet("{id}/urlBuilder")]
public async Task<IActionResult> UrlBuilder([FromRoute] string id)
    {
        var url = $"http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/{id}&acess_token=securetoken";
        return Ok(url);
    }
}
