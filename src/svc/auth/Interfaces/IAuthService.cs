using Auth.Dto;
namespace Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct);
    Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct);
}