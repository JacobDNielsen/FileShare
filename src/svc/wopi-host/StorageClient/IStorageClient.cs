public interface IStorageClient
{
    Task<IReadOnlyList<object>> GetAllMetadataAsync(CancellationToken ct);
    Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct);
    Task<Stream> GetContentsAsync(string fileId, CancellationToken ct);
    Task<HttpResponseMessage> UploadAsync(Stream content, string fileName, CancellationToken ct);
    Task<bool> RenameAsync(string fileId, string newName, CancellationToken ct);
    Task<bool> DeleteAsync(string fileId, CancellationToken ct);
}