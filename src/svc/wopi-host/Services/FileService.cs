using Microsoft.EntityFrameworkCore;
using WopiHost.Data;
using WopiHost.Models;
using Microsoft.AspNetCore.Http;
using WopiHost.dto;

namespace WopiHost.Services;

public class FileService
{
    private readonly WopiDbContext _dbContext;
    private readonly string _storagePath;

    public FileService(WopiDbContext dbContext)
    {
        _dbContext = dbContext;
        var storageRoot = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        _storagePath = Path.Combine(storageRoot, "Files");

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

    public async Task<FileMetadata?> CheckFileInfoAsync(string fileId, CancellationToken ct)
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
            BaseFileName = file.FileName,
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
        var metadata = await CheckFileInfoAsync(fileId, ct);
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
        return (stream, metadata.BaseFileName);
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken ct)
    {
        var entity = await _dbContext.Files.FirstOrDefaultAsync(f => f.FileId == fileId, ct);
        if (entity == null)
            throw new FileNotFoundException($"File with ID '{fileId}' was not found.");

        var filePath = PathFor(fileId);

        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (IOException ex)
        {
            //her kan vi indsætte logik som 'hvis filen er låst' etc..
            throw new InvalidOperationException($"File is in use or locked: {ex.Message}", ex);
        }

        _dbContext.Files.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<string>> DeleteAllFilesAsync(CancellationToken ct)
    {
        var deletedNames = await _dbContext.Files
            .AsNoTracking()
            .Select(f => f.BaseFileName)
            .ToListAsync(ct);

        // vi håndtere ikke errors endnu...
        // sletter filer fra disken/serveren
        if (Directory.Exists(_storagePath))
        {
            foreach (var path in Directory.EnumerateFiles(_storagePath, "*.bin"))
            {
                try { File.Delete(path); } catch { /* ignore */ }
            }
        }

        // 3) sletter DB rows
        await _dbContext.Files.ExecuteDeleteAsync(ct);

        // 4) Hand back the names that were in the DB
        return deletedNames;
    }





    public async Task<FileMetadata?> RenameFileAsync(string fileId, string newName, CancellationToken ct)
    {
        var entity = await _dbContext.Files.FirstOrDefaultAsync(f => f.FileId == fileId, ct);
        if (entity == null)
            return null;

        var oldFilePath = PathFor(fileId);
        var newFilePath = Path.Combine(_storagePath, $"{fileId}.bin");

        try
        {
            //opdater navn i databasen kun...
            entity.BaseFileName = newName;
            entity.LastModifiedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to rename file: {ex.Message}", ex);
        }

        return entity;
    }
}