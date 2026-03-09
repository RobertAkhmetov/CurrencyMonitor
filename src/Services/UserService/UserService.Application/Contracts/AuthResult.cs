namespace UserService.Application.Contracts;

public sealed record AuthResult(
    int UserId,
    string UserName,
    string Token,
    DateTime ExpiresAtUtc);
