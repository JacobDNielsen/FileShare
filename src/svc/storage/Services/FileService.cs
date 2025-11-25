using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Models;
using Microsoft.AspNetCore.Http;
using Storage.Dto;
using Storage.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Storage.FileStorage;
using Storage.Helpers;

namespace Storage.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _repo;
    private readonly IFileStorage _storage;

        public FileService(IFileRepository repo, IFileStorage storage)
    {
        _repo = repo;
        _storage = storage;
    }

     public async Task<List<FileMetadata>> GetAllFilesMetadataAsync(CancellationToken ct)
    {
        return await _repo.GetAllFilesMetadataAsync(ct);
    }
        
    public async Task<FileMetadata?> CheckFileInfoAsync(string fileId, CancellationToken ct)
    {
        return await _repo.GetFileMetadataAsync(fileId, ct);
    }


    public async Task<FileMetadata> UploadAsync(Stream content, string fileName, long size, CancellationToken ct)
    {
        var metadata = new FileMetadata
        {
            BaseFileName = fileName,
            Size = size,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        metadata = await _repo.InsertFileMetadataAsync(metadata, ct);

        try
        {
            await _storage.SaveAsync(metadata.FileId, content, ct);
        }
        catch
        {
            // Roll back metadata if storage fails
            await _repo.DeleteFileMetadataAsync(metadata.FileId, ct);
            throw;
        }

        return metadata;
    }
    
    public async Task<FileMetadata> OverwriteAsync(string fileId, Stream content, string fileName, long size, CancellationToken ct)
    {
        var metadata = await _repo.GetFileMetadataAsync(fileId, ct);
        if (metadata is null)
        {
            throw new FileNotFoundException($"No file with id '{fileId}' exists.");
        }
        // Update metadata fields
        metadata.BaseFileName   = fileName;
        metadata.Size           = size;
        metadata.LastModifiedAt = DateTimeOffset.UtcNow;

        // overwrite file on disk, then persist metadata changes
        await _storage.OverwriteAsync(fileId, content, ct);
        await _repo.UpdateFileMetadataAsync(metadata, ct);

        return metadata;
    }

    public async Task<(Stream? Stream, string? FileName)> GetFileAsync(string fileId, CancellationToken ct)
    {
        var metadata = await CheckFileInfoAsync(fileId, ct);
        if (metadata == null) return (null, null);
        
        var stream = await _storage.OpenReadAsync(fileId, ct);
        if (stream is null) return (null, null);

        return (stream, metadata.BaseFileName);
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken ct)
    {
        var entity = await _repo.GetFileMetadataAsync(fileId, ct);
        if (entity == null)
            throw new FileNotFoundException($"File with ID '{fileId}' was not found.");

        try
        {
            await _storage.DeleteAsync(fileId, ct);
            
        }
        catch (IOException ex)
        {
            //her kan vi indsætte logik som 'hvis filen er låst' etc..
            throw new InvalidOperationException($"File is in use or locked: {ex.Message}", ex);
        }

        await _repo.DeleteFileMetadataAsync(fileId, ct);
    }

    public async Task<List<string>> DeleteAllFilesAsync(CancellationToken ct)
    {
        var deletedNames = await _repo.DeleteAllFileMetadataAsync(ct); //db rows
        await _storage.DeleteAllAsync(ct); //fysisk fil
        return deletedNames;
    }


    public async Task<FileMetadata?> RenameFileAsync(string fileId, string newName, CancellationToken ct)
    {
        var entity = await _repo.GetFileMetadataAsync(fileId, ct);
        if (entity == null) return null;

        entity.BaseFileName = newName;
        entity.LastModifiedAt = DateTimeOffset.UtcNow;

        try
        {
            //opdater navn i databasen kun...
            return await _repo.UpdateFileMetadataAsync(entity, ct);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to rename file: {ex.Message}", ex);
        }
    }
    public async Task<PagedResult<FileListItem>> GetFilesPagedAsync(PageQuery q, CancellationToken ct)
    {
        var baseQuery = _repo.Query(); // <-- from repo, not _storage

        return await baseQuery.ToPagedResultAsync<FileMetadata, FileListItem>(
            q,
            orderBy: qy => qy
                .OrderByDescending(f => f.LastModifiedAt)
                .ThenBy(f => f.FileId),
            selector: f => new FileListItem(f.FileId, f.BaseFileName, f.Size, f.LastModifiedAt),
            ct: ct
        );
    }

}