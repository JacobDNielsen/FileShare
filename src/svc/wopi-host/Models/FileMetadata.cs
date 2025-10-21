namespace WopiHost.Models
{
    public class FileMetadata
    {
        public int Id { get; set; } // vores Primary key
        public string FileId { get; set; } = Guid.NewGuid().ToString("N"); // random 32 digit Guid
        public string BaseFileName { get; set; } = null!;
        public long Size { get; set; } //burde mappe til bigint i db
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;


    }
}