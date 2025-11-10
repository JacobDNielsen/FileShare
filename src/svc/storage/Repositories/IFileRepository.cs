using Storage.Models;

namespace Storage.Repositories;

public interface IFileRepository
{
    Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct);
    Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken ct);
    Task<FileMetadata> InsertFileMetadataAsync(FileMetadata metadata, CancellationToken ct);
    Task<FileMetadata?> UpdateFileMetadataAsync(FileMetadata metadata, CancellationToken ct);
    Task<FileMetadata?> DeleteFileMetadataAsync(string fileId, CancellationToken ct);
    Task<List<string>> DeleteAllFileMetadataAsync(CancellationToken ct);
}