using Microsoft.AspNetCore.Mvc;
using Storage.Services;
using Storage.Dto;

namespace WopiHost.Controllers;

[ApiController]
[Route("admin/files")]
public sealed class AdminFilesController : ControllerBase
{
    private readonly IFileService _files;

    public AdminFilesController(IFileService files) => _files = files;

    // GET /admin/files  (List all metadata)
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var all = await _files.GetAllFilesMetadataAsync(ct);
        return Ok(all);
    }

    // POST /admin/files/upload  (Convenience upload)
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] FileUploadReq fileRequest, CancellationToken ct)
    {
        if (fileRequest?.File == null)
            return BadRequest("No file provided.");

        var meta = await _files.UploadAsync(fileRequest.File, ct);
        return Created($"/wopi/files/{meta.FileId}", meta);
    }

    // GET /admin/files/{id}/download  (Convenience download)
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(string id, CancellationToken ct)
    {
        var (stream, name) = await _files.GetFileAsync(id, ct);
        if (stream is null) return NotFound();
        return File(stream, "application/octet-stream", name);
    }

    // DELETE /admin/files/all  (Bulk delete)
    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll(CancellationToken ct)
    {
        var names = await _files.DeleteAllFilesAsync(ct);
        return Ok(new { message = "Files deleted", count = names.Count, deletedNames = names });
    }

    // GET /admin/files/{id}/url  (Helper to build a test URL to an editor)
    [HttpGet("{id}/url")]
    public IActionResult BuildEditorUrl(string id)
    {
        // NOTE: fix 'access_token' spelling; use a real token in production.
        var url = $"http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/{id}&access_token=securetoken";
        return Ok(new { Url = url });
    }
}
