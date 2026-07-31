using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AssistIQ.Tests.Application;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var jwtTokenService = Substitute.For<IJwtTokenService>();

        var user = AppUser.Create("admin@test.com", "Admin", UserRole.Admin, DateTimeOffset.UtcNow);
        user.SetPasswordHash("hashed");

        userRepository.FindActiveByEmailAsync("admin@test.com", Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher.VerifyPassword(user, "Password123!")
            .Returns(true);
        jwtTokenService.CreateToken(user)
            .Returns("token123");

        var currentUser = Substitute.For<ICurrentUser>();
        var service = new AuthService(userRepository, passwordHasher, jwtTokenService, currentUser);

        var result = await service.LoginAsync(new LoginRequest("admin@test.com", "Password123!"), CancellationToken.None);

        result.Token.Should().Be("token123");
        result.User.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldThrowAppException()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var jwtTokenService = Substitute.For<IJwtTokenService>();

        userRepository.FindActiveByEmailAsync("admin@test.com", Arg.Any<CancellationToken>())
            .Returns((AppUser?)null);

        var currentUser = Substitute.For<ICurrentUser>();
        var service = new AuthService(userRepository, passwordHasher, jwtTokenService, currentUser);

        var act = () => service.LoginAsync(new LoginRequest("admin@test.com", "WrongPassword!"), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.ErrorCode == ErrorCodes.Unauthorized);
    }
}
