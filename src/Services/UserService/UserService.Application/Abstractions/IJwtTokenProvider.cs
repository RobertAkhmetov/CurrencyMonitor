using UserService.Application.Contracts;

namespace UserService.Application.Abstractions;

public interface IJwtTokenProvider
{
    AuthResult Create(UserIdentity user);
}
