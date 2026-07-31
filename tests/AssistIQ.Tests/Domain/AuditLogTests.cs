using AssistIQ.Domain.Audit;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class AuditLogTests
{
    [Fact]
    public void Create_WithValidInput_ShouldReturnAuditLog()
    {
        var actorId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var log = AuditLog.Create(
            actorId,
            AuditAction.TicketCreated,
            "Ticket",
            entityId,
            occurredAt,
            beforeJson: null,
            afterJson: "{\"id\":\"123\"}");

        log.Id.Should().NotBeEmpty();
        log.ActorUserId.Should().Be(actorId);
        log.Action.Should().Be(AuditAction.TicketCreated);
        log.EntityName.Should().Be("Ticket");
        log.EntityId.Should().Be(entityId);
        log.OccurredAt.Should().Be(occurredAt);
        log.BeforeJson.Should().BeNull();
        log.AfterJson.Should().Be("{\"id\":\"123\"}");
    }

    [Fact]
    public void Create_WithNullActorUserId_ShouldAllowSystemAudit()
    {
        var log = AuditLog.Create(
            null,
            AuditAction.DraftGenerated,
            "Draft",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        log.ActorUserId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEntityName_ShouldThrow(string? entityName)
    {
        var act = () => AuditLog.Create(
            Guid.NewGuid(),
            AuditAction.TicketCreated,
            entityName!,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Audit entity name is required.");
    }

    [Fact]
    public void Create_TrimsEntityName()
    {
        var log = AuditLog.Create(
            Guid.NewGuid(),
            AuditAction.TicketCreated,
            "  Ticket  ",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        log.EntityName.Should().Be("Ticket");
    }
}
