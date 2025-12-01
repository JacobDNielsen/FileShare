using Lock.Models;

public interface ILockRepository
{
       public Task<List<FileLock>> GetAllLocks(CancellationToken ct);

       public Task<FileLock?> GetLock(string file_id, CancellationToken ct);

       public Task<FileLock> InsertLock(FileLock file_lock, CancellationToken ct);

       public Task<FileLock> UpdateLock(FileLock file_lock, CancellationToken ct);

       public Task<FileLock> Unlock(string file_id, CancellationToken ct);

       public Task<FileLock> Delete(int lock_id, CancellationToken ct);
}