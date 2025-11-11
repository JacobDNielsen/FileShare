using Microsoft.AspNetCore.Mvc;
using WopiHost.Services;
using WopiHost.Models;
using WopiHost.Dto;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFilesMetadata(CancellationToken ct)
    {
        var files = await _fileService.GetAllFilesMetadataAsync(ct);
        return Ok(files);
    }

    [HttpGet("{fileId}/contents")]
    public async Task<IActionResult> GetFile([FromRoute] string fileId, CancellationToken ct)
    {

        //Vi burde tilføje access token til dette senere
        var (stream, fileName) = await _fileService.GetFileAsync(fileId, ct);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", fileName); // application/octet-stream er til at omsætte stream til bytes
        
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> CheckFileInfo([FromRoute] string fileId, CancellationToken ct)
    {
        var metadata = await _fileService.CheckFileInfoAsync(fileId, ct);
        if (metadata == null)
        {
            return NotFound();
        }

        var response = new CheckFileInfoResponse
        {
            BaseFileName = metadata.BaseFileName,
            Size = metadata.Size,
            OwnerId = "user", // Adjust as needed
            UserId = "user",
            Version = metadata.LastModifiedAt.Ticks.ToString(),
            UserCanWrite = true
        };
        
        return Ok(response);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadReq fileRequest, CancellationToken ct)
    {
        if (fileRequest == null || fileRequest.File == null)
        {
            return BadRequest("No file in request :')");
        }

        var metadata = await _fileService.UploadAsync(fileRequest.File, ct);
        return CreatedAtAction(nameof(CheckFileInfo), new { fileId = metadata.FileId }, metadata);
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

    [HttpDelete("all")]
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
            var updated = await _fileService.RenameFileAsync(fileId, request.BaseFileName, ct);
            if (updated == null)
                return NotFound(new { message = $"File with ID '{fileId}' not found." });

            return Ok(new
            {
                message = "File renamed successfully",
                fileId = updated.FileId,
                newName = updated.BaseFileName
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

    [HttpGet("{fileId}/urlBuilder")]
    public async Task<IActionResult> UrlBuilder([FromRoute] string fileId, CancellationToken ct)
    {
        var url = $"http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/{fileId}&acess_token=securetoken";
        return Ok(url);
    }
       
    [HttpGet("paged")]
       public async Task<ActionResult<PagedResult<FileListItem>>> List(
        [FromQuery] PageQuery q,
        CancellationToken ct)
    {
        var result = await _fileService.GetFilesPagedAsync(q, ct);

        return Ok(result);
    }
}