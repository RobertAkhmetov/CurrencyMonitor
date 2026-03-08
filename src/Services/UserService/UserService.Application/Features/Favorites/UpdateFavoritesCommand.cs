using MediatR;

namespace UserService.Application.Features.Favorites;

public sealed record UpdateFavoritesCommand(int UserId, IReadOnlyCollection<string> Favorites) : IRequest<IReadOnlyCollection<string>>;
