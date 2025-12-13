public interface ILockClient
{
    Task<GetLockResponse?> GetLockAsync(string file_id, CancellationToken ct);
    Task<LockResponse> SetLockAsync(string file_id, LockRequest dto, CancellationToken ct);
    Task<LockResponse> UnlockAsync(string file_id, LockRequest dto, CancellationToken ct);
    Task<LockResponse> RefreshLockAsync(string file_id, LockRequest dto, CancellationToken ct);
}