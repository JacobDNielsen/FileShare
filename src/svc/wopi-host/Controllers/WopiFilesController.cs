using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetContents(string id, CancellationToken ct)
    {
        var stream = await _storage.GetContentsAsync(id, ct);
        return File(stream, "application/octet-stream");
    }

    // Convenience: Upload (pass-through)
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest("No file.");
        await using var s = file.OpenReadStream();
        var resp = await _storage.UploadAsync(s, file.FileName, ct);
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, await resp.Content.ReadAsStringAsync(ct));
        var created = await resp.Content.ReadAsStringAsync(ct);
        return StatusCode((int)resp.StatusCode, created); // 201 with created metadata from Storage
    }

    [HttpPost("{id}/rename")]
    public async Task<IActionResult> Rename(string id, [FromBody] string newName, CancellationToken ct)
        => await _storage.RenameAsync(id, newName, ct) ? Ok() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => await _storage.DeleteAsync(id, ct) ? Ok() : NotFound();
}
