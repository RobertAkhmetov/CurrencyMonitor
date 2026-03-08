using FinanceService.Application.Abstractions;
using FinanceService.Application.Contracts;
using MediatR;

namespace FinanceService.Application.Features.Rates;

public sealed record GetUserRatesQuery(int UserId) : IRequest<IReadOnlyCollection<CurrencyRateDto>>;

public sealed class GetUserRatesQueryHandler(IFinanceRepository financeRepository)
    : IRequestHandler<GetUserRatesQuery, IReadOnlyCollection<CurrencyRateDto>>
{
    public Task<IReadOnlyCollection<CurrencyRateDto>> Handle(GetUserRatesQuery request, CancellationToken cancellationToken)
        => financeRepository.GetRatesByUserAsync(request.UserId, cancellationToken);
}
