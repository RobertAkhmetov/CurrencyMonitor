using MediatR;
using UserService.Application.Abstractions;

namespace UserService.Application.Features.Logout;

public sealed record LogoutCommand(string Jti, DateTime ExpiresAtUtc) : IRequest;

public sealed class LogoutCommandHandler(IUserRepository userRepository) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        => userRepository.RevokeTokenAsync(request.Jti, request.ExpiresAtUtc, cancellationToken);
}
