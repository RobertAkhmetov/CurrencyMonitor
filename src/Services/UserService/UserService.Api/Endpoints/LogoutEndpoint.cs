using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Features.Logout;

namespace UserService.Api.Endpoints;

internal sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/logout", async (ClaimsPrincipal principal, ISender sender, CancellationToken ct) =>
        {
            var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var expiresAtRaw = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
            if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(expiresAtRaw) ||
                !long.TryParse(expiresAtRaw, out var expUnix))
            {
                return Results.BadRequest(new { message = "Token payload is invalid." });
            }

            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            await sender.Send(new LogoutCommand(jti, expiresAtUtc), ct);
            return Results.Ok();
        }).RequireAuthorization();
}
