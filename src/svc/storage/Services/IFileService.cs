using Storage.Models;
using Storage.Dto;

namespace Storage.Services;

public interface IFileService
{
    Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct);
    Task<FileMetadata?> CheckFileInfoAsync(string fileId, CancellationToken ct);
    Task<FileMetadata> UploadAsync(Stream content, string FileName, /*int ownerId,*/ long size, CancellationToken ct);
    Task<(Stream? Stream, string? FileName)> GetFileAsync(string fileId, CancellationToken ct);
    Task DeleteFileAsync(string fileId, CancellationToken ct);
    Task<List<string>> DeleteAllFilesAsync(CancellationToken ct);
    Task<FileMetadata?> RenameFileAsync(string fileId, string newName, CancellationToken ct);
    Task<PagedResult<FileListItem>> GetFilesPagedAsync(PageQuery q, CancellationToken ct);
    Task<FileMetadata> OverwriteAsync(string fileId, Stream content, string fileName, long size, CancellationToken ct);
}
