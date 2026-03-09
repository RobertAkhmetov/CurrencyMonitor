using MediatR;
using UserService.Api.Contracts;
using UserService.Application.Exceptions;
using UserService.Application.Features.Register;

namespace UserService.Api.Endpoints;

internal sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/register", async (RegisterRequest request, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var response = await sender.Send(
                    new RegisterUserCommand(request.Name, request.Password), ct);
                return Results.Ok(response);
            }
            catch (ConflictException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });
}
