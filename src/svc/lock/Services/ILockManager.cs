using Lock.Models;
public interface ILockManager
{
    Task<LockOperationResult> LockAsync(string fileId, string requestedLock, CancellationToken ct);

    Task<LockOperationResult> RefreshLockAsync(string fileId, string requestedLock, CancellationToken ct);

    Task<LockOperationResult> UnlockAsync(string fileId, string requestedLock, CancellationToken ct);

    Task<FileLock?> GetLockAsync(string fileId, CancellationToken ct);
}