namespace Storage.Models;

public class FileLock
{
    public int Id { get; set; } // PK
    public string FileId { get; set; } = null!; // foreign key til FileMetadata.FileId
    public string LockId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30);
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}