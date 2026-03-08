namespace UserService.Application.Contracts;

public sealed record UserIdentity(
    int UserId,
    string Name,
    string PasswordHash,
    IReadOnlyCollection<string> Favorites);
