using FinanceService.Application.Contracts;
using MediatR;

namespace FinanceService.Application.Features.Rates;

public sealed record GetUserRatesQuery(int UserId) : IRequest<IReadOnlyCollection<CurrencyRateDto>>;
