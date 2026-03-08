using MediatR;
using UserService.Application.Abstractions;
using UserService.Application.Exceptions;

namespace UserService.Application.Features.Favorites;

public sealed record UpdateFavoritesCommand(int UserId, IReadOnlyCollection<string> Favorites) : IRequest<IReadOnlyCollection<string>>;

public sealed class UpdateFavoritesCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateFavoritesCommand, IReadOnlyCollection<string>>
{
    public async Task<IReadOnlyCollection<string>> Handle(UpdateFavoritesCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("User is not found.");
        }

        var normalizedFavorites = request.Favorites
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        return await userRepository.UpdateFavoritesAsync(request.UserId, normalizedFavorites, cancellationToken);
    }
}
