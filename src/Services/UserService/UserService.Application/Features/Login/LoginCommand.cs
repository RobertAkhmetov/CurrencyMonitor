using MediatR;
using UserService.Application.Contracts;

namespace UserService.Application.Features.Login;

public sealed record LoginCommand(string Name, string Password) : IRequest<AuthResult>;
