using FinanceService.Application.Features.Rates;
using MediatR;
using System.Security.Claims;

namespace FinanceService.Api.Endpoints;

internal sealed class RatesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/rates", async (ClaimsPrincipal principal, ISender sender, CancellationToken ct) =>
        {
            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var response = await sender.Send(new GetUserRatesQuery(userId), ct);
            return Results.Ok(response);
        }).RequireAuthorization();
}
