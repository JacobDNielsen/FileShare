using Microsoft.EntityFrameworkCore;
using WopiHost.Data;
using WopiHost.Models;
using Microsoft.AspNetCore.Http;

namespace WopiHost.Services;

public class FileService
{
    private readonly WopiDbContext _dbContext;
    private readonly string _storagePath;

    public FileService(WopiDbContext dbContext)
    {
        _dbContext = dbContext;
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        Directory.CreateDirectory(_storagePath);
    }

    private string PathFor(string fileId)
    {
        return Path.Combine(_storagePath, $"{fileId}.bin");
    }

    public async Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct)
    {
        return await _dbContext.Files.AsNoTracking().ToListAsync(ct);
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken ct)
    {
        return await _dbContext.Files.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId, ct);
    }

    public async Task<FileMetadata> UploadAsync(IFormFile file, CancellationToken ct)
    {
        // if (file.Length == 0)
        // {
        //     throw new ArgumentException("File is empty", nameof(file));
        // }

        var metadata = new FileMetadata
        {
            FileName = file.FileName,
            Size = file.Length,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Files.Add(metadata);
        await _dbContext.SaveChangesAsync(ct);

        var filePath = PathFor(metadata.FileId);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, ct);
        }

        return metadata;
    }
    
    public async Task<(Stream? Stream, string? FileName)> GetFileAsync(string fileId, CancellationToken ct)
    {
        var metadata = await GetFileMetadataAsync(fileId, ct);
        if (metadata == null)
        {
            return (null, null);
        }

        var filePath = PathFor(fileId);
        if (!File.Exists(filePath))
        {
            return (null, null);
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, metadata.FileName);
    }
}