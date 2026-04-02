

public interface IOpenFgaService
{
    Task<bool> CanViewFileAsync(string userId, string FileId, CancellationToken cancellationToken = default);
    Task<bool> CanEditFileAsync(string userId, string fileId, CancellationToken cancellationToken = default);
}