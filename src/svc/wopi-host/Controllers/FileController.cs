using Microsoft.AspNetCore.Mvc;
using Storage.Services;
using Storage.Dto;

namespace WopiHost.Controllers;

[ApiController]
[Route("wopi/files")]
public sealed class WopiFilesController : ControllerBase
{
    private readonly IFileService _files;

    public WopiFilesController(IFileService files) => _files = files;

    // GET /wopi/files/{id}  (CheckFileInfo)
    [HttpGet("{id}")]
    public async Task<IActionResult> CheckFileInfo(string id, CancellationToken ct)
    {
        // TODO: validate access_token & proof keys per WOPI before proceeding.
        var meta = await _files.CheckFileInfoAsync(id, ct);
        if (meta is null) return NotFound();

        // Minimal WOPI JSON (extend as needed)
        var response = new CheckFileInfoResponse
        {
            BaseFileName = meta.BaseFileName,
            Size = meta.Size,
            OwnerId = "user",
            UserId = "user",
            Version = meta.LastModifiedAt.Ticks.ToString(),
            UserCanWrite = true
        };

        return Ok(response);
    }

    // GET /wopi/files/{id}/contents  (Download)
    [HttpGet("{id}/contents")]
    public async Task<IActionResult> GetFile(string id, CancellationToken ct)
    {
        var (stream, fileName) = await _files.GetFileAsync(id, ct);
        if (stream is null) return NotFound();
        return File(stream, "application/octet-stream", fileName);
    }

    // POST /wopi/files/{id}/contents  with X-WOPI-Override: PUT  (Update file bytes)
    [HttpPost("{id}/contents")]
    public async Task<IActionResult> PutFile(string id, CancellationToken ct)
    {
        var op = Request.Headers["X-WOPI-Override"].ToString();
        if (!string.Equals(op, "PUT", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Expect X-WOPI-Override: PUT");

        // TODO (next step): implement overwrite path in IFileService (e.g., OverwriteAsync(id, Request.Body, ct))
        // Also validate X-WOPI-Lock vs your stored lock before writing; on mismatch return 409 and set X-WOPI-Lock header.
        return StatusCode(501, "PUT not implemented yet. Add OverwriteAsync to IFileService and handle locks.");
    }

    // POST /wopi/files/{id}  with X-WOPI-Override for LOCK/UNLOCK/REFRESH_LOCK/GET_LOCK/RENAME_FILE/PUT_RELATIVE/DELETE
    [HttpPost("{id}")]
    public async Task<IActionResult> FileOps(string id, CancellationToken ct)
    {
        var op = Request.Headers["X-WOPI-Override"].ToString().ToUpperInvariant();

        switch (op)
        {
            case "RENAME_FILE":
            {
                var requested = Request.Headers["X-WOPI-RequestedName"].ToString();
                if (string.IsNullOrWhiteSpace(requested))
                    return BadRequest("Missing X-WOPI-RequestedName");

                var updated = await _files.RenameFileAsync(id, requested, ct);
                if (updated is null) return NotFound();

                // WOPI expects { Name: "<final name>" }
                return Ok(new { Name = updated.BaseFileName });
            }

            case "DELETE":
            {
                await _files.DeleteFileAsync(id, ct);
                return Ok();
            }

            case "LOCK":
            case "UNLOCK":
            case "REFRESH_LOCK":
            case "GET_LOCK":
            {
                // TODO: Implement your FileLocks table use here.
                // - Validate/echo X-WOPI-Lock
                // - On mismatch: return 409 and set Response.Headers["X-WOPI-Lock"] = "<current-lock>"
                // - Refresh sets new expiry
                // For now, stub success:
                return Ok();
            }

            case "PUT_RELATIVE":
            {
                // TODO: Create sibling file based on X-WOPI-SuggestedTarget or X-WOPI-RelativeTarget
                // Read Request.Body as bytes for the new file content.
                return StatusCode(501, "PUT_RELATIVE not implemented yet.");
            }

            default:
                return BadRequest("Unsupported X-WOPI-Override");
        }
    }
}
