using WopiHost.Dto;
namespace WopiHost.StorageClient;

public interface IStorageClient
{
    Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct);
    Task<Stream> GetFile(string fileId, CancellationToken ct);

}