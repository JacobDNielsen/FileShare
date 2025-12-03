namespace Storage.Dto;
public class CheckFileInfoResponse
{
    public string BaseFileName { get; set; } = null!;
    public long Size { get; set; }
    public int? OwnerId { get; set; } = null!;
    public int UserId { get; set; } = default!;
    public string Version { get; set; } = null!;
    public bool UserCanWrite { get; set; } = false;
}