public class CheckFileInfoResponse
{
    public string BaseFileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string OwnerId { get; set; } = "user";
    public string UserId { get; set; } = "user";
    public string Version { get; set; } = "1";
    public bool UserCanWrite { get; set; } = true;
}