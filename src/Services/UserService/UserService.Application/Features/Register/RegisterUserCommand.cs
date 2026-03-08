using MediatR;
using UserService.Application.Contracts;

namespace UserService.Application.Features.Register;

public sealed record RegisterUserCommand(string Name, string Password, IReadOnlyCollection<string> Favorites) : IRequest<AuthResult>;
