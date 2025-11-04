using User.Dto;
namespace User.Services;

public interface IUserService
{
    Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct);
    Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct);
}