using Microsoft.AspNetCore.Http;

namespace Storage.FileStorage;

public interface IFileStorage
{
    Task SaveAsync(string fileId, IFormFile file, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string fileId, CancellationToken ct);
    Task OverwriteAsync(string fileId, IFormFile file, CancellationToken ct);
    Task DeleteAsync(string fileId, CancellationToken ct);
    Task<int> DeleteAllAsync(CancellationToken ct);

    //helper
    Task<bool> ExistsAsync(string fileId, CancellationToken ct);
    string GetPath(string fileId);
}