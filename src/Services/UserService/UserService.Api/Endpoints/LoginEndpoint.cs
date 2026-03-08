using MediatR;
using UserService.Api.Contracts;
using UserService.Application.Exceptions;
using UserService.Application.Features.Login;

namespace UserService.Api.Endpoints;

internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/login", async (LoginRequest request, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var response = await sender.Send(new LoginCommand(request.Name, request.Password), ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedException)
            {
                return Results.Unauthorized();
            }
        });
}
