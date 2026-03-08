using FinanceService.Application.Abstractions;
using FinanceService.Application.Contracts;
using FinanceService.Application.Features.Rates;
using FluentAssertions;
using Moq;

namespace FinanceService.Tests;

public sealed class GetUserRatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRatesFromRepository()
    {
        var repository = new Mock<IFinanceRepository>();
        repository.Setup(x => x.GetRatesByUserAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRateDto>
            {
                new("USD", 92.123456d),
                new("EUR", 101.654321d)
            });

        var handler = new GetUserRatesQueryHandler(repository.Object);
        var result = await handler.Handle(new GetUserRatesQuery(7), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain(["USD", "EUR"]);
    }
}
