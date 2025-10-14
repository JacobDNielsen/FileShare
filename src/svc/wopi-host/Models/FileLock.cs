namespace WopiHost.Models;

public class FileLock
{
    public int Id { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string LockId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30);
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}