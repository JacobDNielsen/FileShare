public class LockOperationResult
{
    public bool Success { get; set; }
    public string? ExistingLock { get; set; } 
    public string? Reason { get; set; }       // Optional message
}