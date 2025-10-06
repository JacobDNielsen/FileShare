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
        if (fileRequest == null || fileRequest.File == null || fileRequest.File.Length == 0)
        {
            return BadRequest("No file in request or file is empty :')");
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
    
}