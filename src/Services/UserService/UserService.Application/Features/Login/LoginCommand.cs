using MediatR;
using UserService.Application.Abstractions;
using UserService.Application.Contracts;
using UserService.Application.Exceptions;

namespace UserService.Application.Features.Login;

public sealed record LoginCommand(string Name, string Password) : IRequest<AuthResult>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByNameAsync(request.Name.Trim(), cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        var isValidPassword = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        return jwtTokenProvider.Create(user);
    }
}
