using FluentAssertions;
using Moq;
using UserService.Application.Abstractions;
using UserService.Application.Contracts;
using UserService.Application.Exceptions;
using UserService.Application.Features.Register;
using Xunit;

namespace UserService.Tests;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUserAlreadyExists()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.ExistsByNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenProvider = new Mock<IJwtTokenProvider>();

        var handler = new RegisterUserCommandHandler(userRepository.Object, passwordHasher.Object, tokenProvider.Object);
        var command = new RegisterUserCommand("alice", "pass", []);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenUserCreated()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.ExistsByNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository.Setup(x => x.CreateAsync("alice", "hash", It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentity(1, "alice", "hash", ["USD"]));

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Hash("pass")).Returns("hash");

        var tokenProvider = new Mock<IJwtTokenProvider>();
        tokenProvider.Setup(x => x.Create(It.IsAny<UserIdentity>()))
            .Returns(new AuthResult(1, "alice", "token", DateTime.UtcNow.AddHours(1), ["USD"]));

        var handler = new RegisterUserCommandHandler(userRepository.Object, passwordHasher.Object, tokenProvider.Object);
        var command = new RegisterUserCommand("alice", "pass", ["usd"]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Token.Should().Be("token");
        result.Favorites.Should().ContainSingle().Which.Should().Be("USD");
    }
}
