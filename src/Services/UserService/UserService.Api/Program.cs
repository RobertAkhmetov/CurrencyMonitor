using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Api.Contracts;
using UserService.Application;
using UserService.Application.Abstractions;
using UserService.Application.Exceptions;
using UserService.Application.Features.Favorites;
using UserService.Application.Features.Login;
using UserService.Application.Features.Logout;
using UserService.Application.Features.Register;
using UserService.Infrastructure;
using UserService.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddUserApplication();
builder.Services.AddUserInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Jwt options are not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("Token jti is missing.");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var isRevoked = await userRepository.IsTokenRevokedAsync(jti, context.HttpContext.RequestAborted);
                if (isRevoked)
                {
                    context.Fail("Token is revoked.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/user/register", async (RegisterRequest request, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        var response = await sender.Send(new RegisterUserCommand(request.Name, request.Password, request.Favorites ?? []), cancellationToken);
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

app.MapPost("/api/user/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        var response = await sender.Send(new LoginCommand(request.Name, request.Password), cancellationToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/user/logout", async (ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken) =>
{
    var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
    var expiresAtRaw = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
    if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(expiresAtRaw) || !long.TryParse(expiresAtRaw, out var expUnix))
    {
        return Results.BadRequest(new { message = "Token payload is invalid." });
    }

    var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
    await sender.Send(new LogoutCommand(jti, expiresAtUtc), cancellationToken);
    return Results.Ok();
}).RequireAuthorization();

app.MapPut("/api/user/favorites", async (UpdateFavoritesRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken) =>
{
    var userIdRaw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdRaw, out var userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var response = await sender.Send(new UpdateFavoritesCommand(userId, request.Favorites), cancellationToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedException)
    {
        return Results.Unauthorized();
    }
}).RequireAuthorization();

app.Run();
