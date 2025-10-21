namespace WopiHost.Dto
{
    public class FileUploadReq
    {
        public IFormFile File { get; set; } = default!; //defaults to null, supresses null warning
    }
}