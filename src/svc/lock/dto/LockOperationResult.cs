public class LockOperationResult
{
        /// <summary>
        /// Indicates whether the lock operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The existing lock value, if any, that prevented the operation from succeeding.
        /// </summary>
        public string? ExistingLock { get; set; }

        /// <summary>
        /// Optional message describing why the operation failed.
        /// </summary>
        public string? Reason { get; set; }
}