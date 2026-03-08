using System.Security.Claims;
using MediatR;
using UserService.Api.Contracts;
using UserService.Application.Exceptions;
using UserService.Application.Features.Favorites;

namespace UserService.Api.Endpoints;

internal sealed class FavoritesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/favorites", async (UpdateFavoritesRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken ct) =>
        {
            var userIdRaw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdRaw, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var response = await sender.Send(new UpdateFavoritesCommand(userId, request.Favorites), ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedException)
            {
                return Results.Unauthorized();
            }
        }).RequireAuthorization();
}
