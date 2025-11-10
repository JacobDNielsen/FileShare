using Microsoft.AspNetCore.Http;

namespace Storage.FileStorage;

public sealed class FileStorage : IFileStorage
{
    private readonly string _root;

    // Optional: allow DI config later. For now default to "Data/Files"
    public FileStorage(string? rootPath = null)
    {
        var basePath = rootPath is { Length: > 0 }
            ? rootPath
            : Path.Combine(Directory.GetCurrentDirectory(), "Data", "Files");

        _root = Path.IsPathRooted(basePath)
            ? basePath
            : Path.GetFullPath(basePath);

        Directory.CreateDirectory(_root);
    }

    private string PathFor(string fileId) => Path.Combine(_root, $"{fileId}.bin");

    public string GetPath(string fileId) => PathFor(fileId);

    public async Task SaveAsync(string fileId, IFormFile file, CancellationToken ct)
    {
        var path = PathFor(fileId);
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(fs, ct);
    }

    public Task<Stream?> OpenReadAsync(string fileId, CancellationToken ct)
    {
        var path = PathFor(fileId);
        if (!File.Exists(path)) return Task.FromResult<Stream?>(null);
        Stream s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(s);
    }

    public async Task OverwriteAsync(string fileId, IFormFile file, CancellationToken ct)
    {
        var path = PathFor(fileId);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(fs, ct);
    }

    public Task DeleteAsync(string fileId, CancellationToken ct)
    {
        var path = PathFor(fileId);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

     public Task<int> DeleteAllAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_root)) return Task.FromResult(0);
        var files = Directory.EnumerateFiles(_root, "*.bin").ToList();
        var count = 0;
        foreach (var f in files)
        {
            try { File.Delete(f); count++; } catch { /* ignore */ }
        }
        return Task.FromResult(count);
    }
    
    public Task<bool> ExistsAsync(string fileId, CancellationToken ct) =>
        Task.FromResult(File.Exists(PathFor(fileId)));

}