using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Models;
using Storage.Repositories;

public sealed class FileRepository : IFileRepository
{
    private readonly WopiDbContext _dbContext;

    //private readonly string _storagePath;

    public FileRepository(WopiDbContext dbContext) => _dbContext = dbContext;


    public Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct) =>
        _dbContext.Files.AsNoTracking().ToListAsync(ct);

    public Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken ct) =>
        _dbContext.Files.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId, ct);

    public async Task<FileMetadata> InsertFileMetadataAsync(FileMetadata metadata, CancellationToken ct)
    {
        _dbContext.Files.Add(metadata);
        await _dbContext.SaveChangesAsync(ct);
        return metadata;
    }

    public async Task<FileMetadata?> UpdateFileMetadataAsync(FileMetadata metadata, CancellationToken ct)
    {
        _dbContext.Files.Update(metadata);
        await _dbContext.SaveChangesAsync(ct);
        return metadata;
    }

    public async Task<FileMetadata?> DeleteFileMetadataAsync(string fileId, CancellationToken ct)
    {
        var entity = await _dbContext.Files.FirstOrDefaultAsync(f => f.FileId == fileId, ct);
        if (entity is null) return null;

        _dbContext.Files.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<List<string>> DeleteAllFileMetadataAsync(CancellationToken ct)
    {
        var names = await _dbContext.Files.AsNoTracking().Select(f => f.BaseFileName).ToListAsync(ct);
        await _dbContext.Files.ExecuteDeleteAsync(ct);
        return names;
    }


}