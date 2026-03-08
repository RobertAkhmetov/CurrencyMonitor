using UserService.Application.Contracts;

namespace UserService.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task<UserIdentity?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<UserIdentity?> GetByIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserIdentity> CreateAsync(string name, string passwordHash, IReadOnlyCollection<string> favorites, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> UpdateFavoritesAsync(int userId, IReadOnlyCollection<string> favorites, CancellationToken cancellationToken);
    Task RevokeTokenAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken);
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken);
}
