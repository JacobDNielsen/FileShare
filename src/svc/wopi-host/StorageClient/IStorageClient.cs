using WopiHost.Dto;

public interface IStorageClient
{
    Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct);
    Task<Stream> GetFile(string fileId, CancellationToken ct);

}