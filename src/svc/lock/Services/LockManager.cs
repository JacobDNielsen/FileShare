
using Lock.Models;


public class LockManager : ILockManager
{
    private readonly ILockRepository _repo;

    public LockManager(ILockRepository repo)
    {
        _repo = repo;
    }

    public async Task<LockOperationResult> LockAsync(string fileId, string requestedLock, CancellationToken ct)
    {
        var existing = await _repo.GetLock(fileId, ct);

    
        if (existing != null && existing.ExpiresAt < DateTimeOffset.UtcNow)
        {
            await _repo.Unlock(fileId, ct);
            existing = null;
        }

        if (existing == null)
        {
           
            var fileLock = new FileLock
            {
                FileId = fileId,
                LockId = requestedLock,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            await _repo.InsertLock(fileLock, ct);

            return new LockOperationResult { Success = true };
        }

        if (existing.LockId == requestedLock)
        {
            
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
            await _repo.UpdateLock(existing, ct);

            return new LockOperationResult { Success = true };
        }

     
        return new LockOperationResult
        {
            Success = false,
            ExistingLock = existing.LockId,
            Reason = "Lock mismatch."
        };
    }

    public async Task<LockOperationResult> RefreshLockAsync(string fileId, string requestedLock, CancellationToken ct)
    {
        var existing = await _repo.GetLock(fileId, ct);

        if (existing == null || existing.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return new LockOperationResult
            {
                Success = false,
                ExistingLock = existing?.LockId
            };
        }

        if (existing.LockId != requestedLock)
        {
            return new LockOperationResult
            {
                Success = false,
                ExistingLock = existing.LockId,
                Reason = "Lock mismatch."
            };
        }

      
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        await _repo.UpdateLock(existing, ct);

        return new LockOperationResult { Success = true };
    }

    public async Task<LockOperationResult> UnlockAsync(string fileId, string requestedLock, CancellationToken ct)
    {
        var existing = await _repo.GetLock(fileId, ct);

        if (existing == null || existing.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return new LockOperationResult
            {
                Success = false,
                ExistingLock = null,
                Reason = "No lock exists."
            };
        }

        if (existing.LockId != requestedLock)
        {
            return new LockOperationResult
            {
                Success = false,
                ExistingLock = existing.LockId,
                Reason = "Lock mismatch."
            };
        }

  
        await _repo.Unlock(fileId, ct);

        return new LockOperationResult { Success = true };
    }

    public async Task<FileLock?> GetLockAsync(string fileId, CancellationToken ct)
    {
        var existing = await _repo.GetLock(fileId, ct);

        if (existing == null || existing.ExpiresAt < DateTimeOffset.UtcNow)
        {
            if (existing != null)
                await _repo.Unlock(fileId, ct); // delete expired lock
            return null;
        }

        return existing;
    }
}