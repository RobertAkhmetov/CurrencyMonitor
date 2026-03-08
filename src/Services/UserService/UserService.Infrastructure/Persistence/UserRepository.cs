using CurrencyMonitor.Persistence;
using CurrencyMonitor.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions;
using UserService.Application.Contracts;

namespace UserService.Infrastructure.Persistence;

public sealed class UserRepository(CurrencyMonitorDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.Trim().ToLower();
        return dbContext.Users.AnyAsync(x => x.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<UserIdentity?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.Trim().ToLower();
        var user = await dbContext.Users
            .Include(x => x.FavoriteCurrencies)
            .ThenInclude(x => x.Currency)
            .SingleOrDefaultAsync(x => x.Name.ToLower() == normalized, cancellationToken);

        return user is null ? null : Map(user);
    }

    public async Task<UserIdentity?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.FavoriteCurrencies)
            .ThenInclude(x => x.Currency)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        return user is null ? null : Map(user);
    }

    public async Task<UserIdentity> CreateAsync(string name, string passwordHash, IReadOnlyCollection<string> favorites, CancellationToken cancellationToken)
    {
        var currencies = await GetOrCreateCurrenciesAsync(favorites, cancellationToken);

        var user = new AppUser
        {
            Name = name,
            Password = passwordHash
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var currency in currencies)
        {
            dbContext.FavoriteCurrencies.Add(new FavoriteCurrency
            {
                UserId = user.Id,
                CurrencyId = currency.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<IReadOnlyCollection<string>> UpdateFavoritesAsync(int userId, IReadOnlyCollection<string> favorites, CancellationToken cancellationToken)
    {
        var existing = await dbContext.FavoriteCurrencies.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        dbContext.FavoriteCurrencies.RemoveRange(existing);

        var currencies = await GetOrCreateCurrenciesAsync(favorites, cancellationToken);
        foreach (var currency in currencies)
        {
            dbContext.FavoriteCurrencies.Add(new FavoriteCurrency
            {
                UserId = userId,
                CurrencyId = currency.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return currencies.Select(x => x.Name).OrderBy(x => x).ToArray();
    }

    public async Task RevokeTokenAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        var exists = await dbContext.RevokedTokens.AnyAsync(x => x.Jti == jti, cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.RevokedTokens.Add(new RevokedToken
        {
            Jti = jti,
            ExpiresAtUtc = expiresAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        return dbContext.RevokedTokens.AnyAsync(x => x.Jti == jti, cancellationToken);
    }

    private async Task<List<Currency>> GetOrCreateCurrenciesAsync(IReadOnlyCollection<string> currencyNames, CancellationToken cancellationToken)
    {
        if (currencyNames.Count == 0)
        {
            return [];
        }

        var names = currencyNames.Select(x => x.Trim().ToUpperInvariant()).Distinct().ToArray();
        var existing = await dbContext.Currencies.Where(x => names.Contains(x.Name)).ToListAsync(cancellationToken);
        var existingNames = existing.Select(x => x.Name).ToHashSet();
        var missingNames = names.Where(x => !existingNames.Contains(x)).ToArray();

        foreach (var missing in missingNames)
        {
            dbContext.Currencies.Add(new Currency
            {
                Name = missing,
                Rate = 0d
            });
        }

        if (missingNames.Length > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            existing = await dbContext.Currencies.Where(x => names.Contains(x.Name)).ToListAsync(cancellationToken);
        }

        return existing;
    }

    private static UserIdentity Map(AppUser user)
    {
        var favorites = user.FavoriteCurrencies.Select(x => x.Currency.Name).OrderBy(x => x).ToArray();
        return new UserIdentity(user.Id, user.Name, user.Password, favorites);
    }
}
