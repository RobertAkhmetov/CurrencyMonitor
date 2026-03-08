using MediatR;
using UserService.Application.Abstractions;

namespace UserService.Application.Features.Logout;

public sealed class LogoutCommandHandler(IUserRepository userRepository) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await userRepository.RevokeTokenAsync(request.Jti, request.ExpiresAtUtc, cancellationToken);
    }
}
