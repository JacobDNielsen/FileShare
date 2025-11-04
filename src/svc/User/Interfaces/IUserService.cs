using User.Dto;
namespace User.Interfaces;

public interface IUserService
{
    Task<AuthResp> SignupAsync(SignupReq req, CancellationToken ct);
    Task<AuthResp?> LoginAsync(LoginReq req, CancellationToken ct);
}