using MediatR;

namespace UserService.Application.Features.Logout;

public sealed record LogoutCommand(string Jti, DateTime ExpiresAtUtc) : IRequest;
