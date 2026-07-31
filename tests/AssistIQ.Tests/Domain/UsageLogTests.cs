using AssistIQ.Domain.Usage;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class UsageLogTests
{
    [Fact]
    public void Succeeded_WithValidInput_ShouldReturnSucceededLog()
    {
        var actorId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var draftId = Guid.NewGuid();

        var log = UsageLog.Succeeded(
            actorId, ticketId, draftId,
            "Fake", "fake-model", "resp-123",
            100, 50, 0.5m, DateTimeOffset.UtcNow);

        log.Id.Should().NotBeEmpty();
        log.ActorUserId.Should().Be(actorId);
        log.TicketId.Should().Be(ticketId);
        log.DraftId.Should().Be(draftId);
        log.Provider.Should().Be("Fake");
        log.Model.Should().Be("fake-model");
        log.ResponseId.Should().Be("resp-123");
        log.PromptTokens.Should().Be(100);
        log.CompletionTokens.Should().Be(50);
        log.TotalTokens.Should().Be(150);
        log.EstimatedCost.Should().Be(0.5m);
        log.Status.Should().Be(UsageStatus.Succeeded);
        log.ErrorSummary.Should().BeNull();
    }

    [Fact]
    public void Failed_WithValidInput_ShouldReturnFailedLog()
    {
        var log = UsageLog.Failed(
            Guid.NewGuid(), Guid.NewGuid(), null,
            "Fake", "fake-model", "Connection timeout",
            DateTimeOffset.UtcNow);

        log.Status.Should().Be(UsageStatus.Failed);
        log.ErrorSummary.Should().Be("Connection timeout");
        log.PromptTokens.Should().Be(0);
        log.CompletionTokens.Should().Be(0);
        log.TotalTokens.Should().Be(0);
        log.EstimatedCost.Should().Be(0m);
        log.ResponseId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Succeeded_WithEmptyProvider_ShouldThrow(string? provider)
    {
        var act = () => UsageLog.Succeeded(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            provider!, "model", "resp-1",
            10, 10, 0m, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Usage provider is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Succeeded_WithEmptyModel_ShouldThrow(string? model)
    {
        var act = () => UsageLog.Succeeded(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Fake", model!, "resp-1",
            10, 10, 0m, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Usage model is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Succeeded_WithEmptyResponseId_ShouldThrow(string? responseId)
    {
        var act = () => UsageLog.Succeeded(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Fake", "model", responseId!,
            10, 10, 0m, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Usage response id is required.");
    }

    [Fact]
    public void Succeeded_WithNegativeTokens_ShouldThrow()
    {
        var act = () => UsageLog.Succeeded(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Fake", "model", "resp-1",
            -1, 10, 0m, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Usage token counts cannot be negative.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Failed_WithEmptyErrorSummary_ShouldThrow(string? errorSummary)
    {
        var act = () => UsageLog.Failed(
            Guid.NewGuid(), Guid.NewGuid(), null,
            "Fake", "model", errorSummary!,
            DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Usage error summary is required.");
    }
}
