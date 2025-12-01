using Microsoft.EntityFrameworkCore;
using Lock.Data;
using Lock.Models;



public class LockRepository : ILockRepository
{
    private readonly WopiDbContext _dbContext;

    public LockRepository(WopiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<FileLock>> GetAllLocks(CancellationToken ct) =>
        _dbContext.FileLocks.AsNoTracking().ToListAsync(ct);

    public Task<FileLock?> GetLock(string file_id, CancellationToken ct) =>
        _dbContext.FileLocks.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileId == file_id, ct);

    public async Task<FileLock> InsertLock(FileLock file_lock, CancellationToken ct)
    {
        _dbContext.FileLocks.Add(file_lock);
        await _dbContext.SaveChangesAsync(ct);
        return file_lock;
    }

    public async Task<FileLock> UpdateLock(FileLock file_lock, CancellationToken ct)
    {
        _dbContext.FileLocks.Update(file_lock);
        await _dbContext.SaveChangesAsync(ct);
        return file_lock;
    }

    public async Task<FileLock?> Unlock(string file_id, CancellationToken ct)
    {
        var existing = await _dbContext.FileLocks
            .FirstOrDefaultAsync(f => f.FileId == file_id, ct);

        if (existing == null)
            return null;

        _dbContext.FileLocks.Remove(existing);
        await _dbContext.SaveChangesAsync(ct);

        return existing;
    }

    public async Task<FileLock?> Delete(int lock_id, CancellationToken ct)
    {
        var entity = await _dbContext.FileLocks.FirstOrDefaultAsync(f => f.Id == lock_id, ct);
        if (entity == null) return null;

        _dbContext.FileLocks.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
        return entity;
    }
}