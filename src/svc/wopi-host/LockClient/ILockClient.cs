public interface ILockClient
{
    Task<HttpResponseMessage> GetLockAsync(string fileId, CancellationToken ct);
    Task<HttpResponseMessage> LockAsync(string fileId, string lockValue, CancellationToken ct);
    Task<HttpResponseMessage> RefreshLockAsync(string fileId, string lockValue, CancellationToken ct);
    Task<HttpResponseMessage> UnlockAsync(string fileId, string lockValue, CancellationToken ct);
}