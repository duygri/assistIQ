using AssistIQ.Domain.Users;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class AppUserTests
{
    [Fact]
    public void Create_WithValidInput_ShouldReturnActiveUser()
    {
        var user = AppUser.Create("admin@test.com", "Admin User", UserRole.Admin, DateTimeOffset.UtcNow);

        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("admin@test.com");
        user.DisplayName.Should().Be("Admin User");
        user.Role.Should().Be(UserRole.Admin);
        user.IsActive.Should().BeTrue();
        user.DisabledAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ShouldThrow(string? email)
    {
        var act = () => AppUser.Create(email!, "Name", UserRole.Admin, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User email is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDisplayName_ShouldThrow(string? displayName)
    {
        var act = () => AppUser.Create("test@test.com", displayName!, UserRole.Admin, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User display name is required.");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var user = AppUser.Create("  admin@test.com  ", "  Admin  ", UserRole.SupportAgent, DateTimeOffset.UtcNow);

        user.Email.Should().Be("admin@test.com");
        user.DisplayName.Should().Be("Admin");
    }

    [Fact]
    public void Disable_ShouldSetInactiveAndTimestamp()
    {
        var user = AppUser.Create("test@test.com", "Test", UserRole.Admin, DateTimeOffset.UtcNow);
        var disabledAt = DateTimeOffset.UtcNow;

        user.Disable(disabledAt);

        user.IsActive.Should().BeFalse();
        user.DisabledAt.Should().Be(disabledAt);
    }

    [Fact]
    public void SetPasswordHash_WithValidHash_ShouldSet()
    {
        var user = AppUser.Create("test@test.com", "Test", UserRole.Admin, DateTimeOffset.UtcNow);

        user.SetPasswordHash("hashed-password-123");

        user.PasswordHash.Should().Be("hashed-password-123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPasswordHash_WithEmptyHash_ShouldThrow(string? hash)
    {
        var user = AppUser.Create("test@test.com", "Test", UserRole.Admin, DateTimeOffset.UtcNow);

        var act = () => user.SetPasswordHash(hash!);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Password hash is required.");
    }
}
