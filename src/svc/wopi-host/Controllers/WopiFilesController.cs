using Microsoft.AspNetCore.Mvc;
using WopiHost.Dto;

[ApiController]
[Route("wopi/files")]
public sealed class WopiFilesController : ControllerBase
{
    private readonly IStorageClient _storage;
    public WopiFilesController(IStorageClient storage) => _storage = storage;

    // WOPI: CheckFileInfo proxy
    [HttpGet("{id}")]
    public async Task<IActionResult> CheckFileInfo(string id, CancellationToken ct)
    {
        var info = await _storage.CheckFileInfoAsync(id, ct);
        return info is null ? NotFound() : Ok(info);
    }

    // WOPI: GetFile (contents)
    [HttpGet("{id}/contents")]
    public async Task<IActionResult> GetFile(string id, CancellationToken ct)
    {
        var stream = await _storage.GetFile(id, ct);
        return File(stream, "application/octet-stream");
    }

    [HttpGet("{fileId}/urlBuilder")]
    public async Task<IActionResult> UrlBuilder([FromRoute] string fileId, CancellationToken ct)
    {
        var url = $"http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/{fileId}&acess_token=securetoken";
        return Ok(url);
    }
}
