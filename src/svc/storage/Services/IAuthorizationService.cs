public interface IAuthorizationService
{
    Task<bool> CanViewFile(string userId, string fileId);
}