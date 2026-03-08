using MediatR;
using UserService.Application.Abstractions;
using UserService.Application.Contracts;
using UserService.Application.Exceptions;

namespace UserService.Application.Features.Register;

public sealed record RegisterUserCommand(string Name, string Password, IReadOnlyCollection<string> Favorites) : IRequest<AuthResult>;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider) : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedException("Name and password are required.");
        }

        var normalizedName = request.Name.Trim();
        var exists = await userRepository.ExistsByNameAsync(normalizedName, cancellationToken);
        if (exists)
        {
            throw new ConflictException("User with this name already exists.");
        }

        var favorites = request.Favorites
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = await userRepository.CreateAsync(normalizedName, passwordHash, favorites, cancellationToken);
        return jwtTokenProvider.Create(user);
    }
}
