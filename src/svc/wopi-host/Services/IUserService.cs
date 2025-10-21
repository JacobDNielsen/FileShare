using WopiHost.Dto;
namespace WopiHost.Services;

public interface IUserService
{
    Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct);
    Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct);
}