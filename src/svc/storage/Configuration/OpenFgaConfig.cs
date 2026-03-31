using System.Net.Http.Json;
using Microsoft.Extensions.Options;

public interface IAuthorizationService
{
    Task<bool> CanViewFile(string userId, string fileId);
}