using System.Security.Claims;
using FinanceService.Application.Features.Rates;
using MediatR;

namespace FinanceService.Api.Endpoints;

internal sealed class RatesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/rates", async (ClaimsPrincipal principal, ISender sender, CancellationToken ct) =>
        {
            var userIdRaw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdRaw, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new GetUserRatesQuery(userId), ct);
            return Results.Ok(response);
        }).RequireAuthorization();
}
