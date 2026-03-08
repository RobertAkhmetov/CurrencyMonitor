using CurrencyMonitor.Persistence;
using FinanceService.Application.Abstractions;
using FinanceService.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Infrastructure.Persistence;

public sealed class FinanceRepository(CurrencyMonitorDbContext dbContext) : IFinanceRepository
{
    public async Task<IReadOnlyCollection<CurrencyRateDto>> GetRatesByUserAsync(int userId, CancellationToken cancellationToken)
    {
        var rates = await dbContext.FavoriteCurrencies
            .Where(x => x.UserId == userId)
            .Include(x => x.Currency)
            .OrderBy(x => x.Currency.Name)
            .Select(x => new CurrencyRateDto(x.Currency.Name, x.Currency.Rate))
            .ToListAsync(cancellationToken);

        return rates;
    }

    public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken) => dbContext.RevokedTokens.AnyAsync(x => x.Jti == jti, cancellationToken);
}
