namespace Auth.Dto;

public sealed record SignupReq(string UserName, string Email, string Password, string? Locale = null); //for future, probably also add string TenantId (for multi-tenant)
public sealed record LoginReq(string UserName, string Password);
public sealed record AuthResp(string UserName, string TokenType, string AccessToken); //For future, should also add Role.