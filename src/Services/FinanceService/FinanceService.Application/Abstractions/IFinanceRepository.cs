using FinanceService.Application.Contracts;

namespace FinanceService.Application.Abstractions;

public interface IFinanceRepository
{
    Task<IReadOnlyCollection<CurrencyRateDto>> GetRatesByUserAsync(int userId, CancellationToken cancellationToken);
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken);
}
