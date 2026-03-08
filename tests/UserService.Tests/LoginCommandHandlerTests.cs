using FluentAssertions;
using Moq;
using UserService.Application.Abstractions;
using UserService.Application.Contracts;
using UserService.Application.Exceptions;
using UserService.Application.Features.Login;
using Xunit;

namespace UserService.Tests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordInvalid()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentity(1, "alice", "hash", []));

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Verify("pass", "hash")).Returns(false);

        var tokenProvider = new Mock<IJwtTokenProvider>();
        var handler = new LoginCommandHandler(userRepository.Object, passwordHasher.Object, tokenProvider.Object);

        var act = () => handler.Handle(new LoginCommand("alice", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResult_WhenCredentialsValid()
    {
        var user = new UserIdentity(1, "alice", "hash", ["EUR"]);
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Verify("pass", "hash")).Returns(true);

        var tokenProvider = new Mock<IJwtTokenProvider>();
        tokenProvider.Setup(x => x.Create(user))
            .Returns(new AuthResult(1, "alice", "token", DateTime.UtcNow.AddHours(1), ["EUR"]));

        var handler = new LoginCommandHandler(userRepository.Object, passwordHasher.Object, tokenProvider.Object);
        var result = await handler.Handle(new LoginCommand("alice", "pass"), CancellationToken.None);

        result.Token.Should().Be("token");
        result.UserName.Should().Be("alice");
    }
}
