using Microsoft.AspNetCore.Mvc;
using WopiHost.Services;
using WopiHost.Models;
using WopiHost.dto;

namespace WopiHost.Controllers;

[ApiController]
[Route("wopi/files")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;

    public FileController(FileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFilesMetadata(CancellationToken ct)
    {
        var files = await _fileService.GetAllFilesMetadataAsync(ct);
        return Ok(files);
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetFileMetadata([FromRoute] string fileId, CancellationToken ct)
    {
        var metadata = await _fileService.GetFileMetadataAsync(fileId, ct);
        if (metadata == null)
        {
            return NotFound();
        }
        return Ok(metadata);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadReq fileRequest, CancellationToken ct)
    {
        if (fileRequest == null || fileRequest.File == null)
        {
            return BadRequest("No file in request :')");
        }

        var metadata = await _fileService.UploadAsync(fileRequest.File, ct);
        return CreatedAtAction(nameof(GetFileMetadata), new { fileId = metadata.FileId }, metadata);
    }

    [HttpGet("{fileId}/download")]
    public async Task<IActionResult> DownloadFile([FromRoute] string fileId, CancellationToken ct)
    {
        var (stream, fileName) = await _fileService.GetFileAsync(fileId, ct);
        if (stream == null)
        {
            return NotFound("no file found with that id :')");
        }

        return File(stream, "application/octet-stream", fileName);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> Delete(string fileId, CancellationToken ct)
    {
        try
        {
            await _fileService.DeleteFileAsync(fileId, ct);
            return Ok(new
            {
                message = "File deleted successfully",
                fileId
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new
            {
                message = $"File with ID '{fileId}' was not found.",
                fileId
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message,
                fileId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred while deleting the file.",
                details = ex.Message,
                fileId
            });
        }
    }

    [HttpDelete("wopi/files")]
    public async Task<IActionResult> DeleteAll(CancellationToken ct)
    {
        var names = await _fileService.DeleteAllFilesAsync(ct);
        return Ok(new { message = "Files deleted", count = names.Count, deletedNames = names });
    }



    [HttpPost("{fileId}/rename")]
    public async Task<IActionResult> Rename(string fileId, [FromBody] RenameRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _fileService.RenameFileAsync(fileId, request.Name, ct);
            if (updated == null)
                return NotFound(new { message = $"File with ID '{fileId}' not found." });

            return Ok(new
            {
                message = "File renamed successfully",
                fileId = updated.FileId,
                newName = updated.FileName
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message, fileId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error while renaming file.", details = ex.Message, fileId });
        }
    }
       
}