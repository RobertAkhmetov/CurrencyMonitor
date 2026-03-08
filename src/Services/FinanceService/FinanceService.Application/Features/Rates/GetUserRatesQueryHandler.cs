using FinanceService.Application.Abstractions;
using FinanceService.Application.Contracts;
using MediatR;

namespace FinanceService.Application.Features.Rates;

public sealed class GetUserRatesQueryHandler(IFinanceRepository financeRepository)
    : IRequestHandler<GetUserRatesQuery, IReadOnlyCollection<CurrencyRateDto>>
{
    public Task<IReadOnlyCollection<CurrencyRateDto>> Handle(GetUserRatesQuery request, CancellationToken cancellationToken)
    {
        return financeRepository.GetRatesByUserAsync(request.UserId, cancellationToken);
    }
}
