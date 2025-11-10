using Microsoft.AspNetCore.Http;
using Storage.Models;

namespace Storage.Services;

public interface IFileService
{
    Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct);
    Task<FileMetadata?> CheckFileInfoAsync(string fileId, CancellationToken ct);
    Task<FileMetadata> UploadAsync(IFormFile file, CancellationToken ct);
    Task<(Stream? Stream, string? FileName)> GetFileAsync(string fileId, CancellationToken ct);
    Task DeleteFileAsync(string fileId, CancellationToken ct);
    Task<List<string>> DeleteAllFilesAsync(CancellationToken ct);
    Task<FileMetadata?> RenameFileAsync(string fileId, string newName, CancellationToken ct);
}
