namespace WopiHost.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileId { get; set; } = Guid.NewGuid().ToString("N");
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;


    }
}