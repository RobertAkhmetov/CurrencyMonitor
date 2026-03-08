namespace UserService.Api.Contracts;

public sealed record RegisterRequest(string Name, string Password, IReadOnlyCollection<string>? Favorites);
public sealed record LoginRequest(string Name, string Password);
public sealed record UpdateFavoritesRequest(IReadOnlyCollection<string> Favorites);
