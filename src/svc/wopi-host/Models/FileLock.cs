namespace WopiHost.Models;

public class FileLock
{
    public int Id { get; set; } // PK
    public string FileId { get; set; } = null!; // foreign key til FileMetadata.FileId
    public string LockId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30); // hvornår lock udløber. Skal være default på 30 ifølge wopi
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}