using AssistIQ.Domain.Tickets;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class TicketTests
{
    [Fact]
    public void Create_WithValidInput_ShouldReturnOpenTicket()
    {
        var ticket = Ticket.Create("How to reset password?", "John", "john@example.com", Guid.NewGuid(), DateTimeOffset.UtcNow);

        ticket.Id.Should().NotBeEmpty();
        ticket.CustomerQuestion.Should().Be("How to reset password?");
        ticket.CustomerName.Should().Be("John");
        ticket.CustomerEmail.Should().Be("john@example.com");
        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.DraftedAt.Should().BeNull();
        ticket.SentAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyQuestion_ShouldThrow(string? question)
    {
        var act = () => Ticket.Create(question!, null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Ticket question is required.");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var ticket = Ticket.Create("  How?  ", "  John  ", "  john@test.com  ", Guid.NewGuid(), DateTimeOffset.UtcNow);

        ticket.CustomerQuestion.Should().Be("How?");
        ticket.CustomerName.Should().Be("John");
        ticket.CustomerEmail.Should().Be("john@test.com");
    }

    [Fact]
    public void Create_WithNullOptionalFields_ShouldSetNull()
    {
        var ticket = Ticket.Create("Question?", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);

        ticket.CustomerName.Should().BeNull();
        ticket.CustomerEmail.Should().BeNull();
    }

    [Fact]
    public void MarkDrafted_FromOpen_ShouldTransitionToDrafted()
    {
        var ticket = Ticket.Create("Question?", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);
        var draftedAt = DateTimeOffset.UtcNow;

        ticket.MarkDrafted(draftedAt);

        ticket.Status.Should().Be(TicketStatus.Drafted);
        ticket.DraftedAt.Should().Be(draftedAt);
    }

    [Fact]
    public void MarkDrafted_FromSent_ShouldThrow()
    {
        var ticket = Ticket.Create("Question?", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);
        ticket.MarkDrafted(DateTimeOffset.UtcNow);
        ticket.MarkSent(DateTimeOffset.UtcNow);

        var act = () => ticket.MarkDrafted(DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Sent tickets cannot be drafted again.");
    }

    [Fact]
    public void MarkSent_FromDrafted_ShouldTransitionToSent()
    {
        var ticket = Ticket.Create("Question?", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);
        ticket.MarkDrafted(DateTimeOffset.UtcNow);
        var sentAt = DateTimeOffset.UtcNow;

        ticket.MarkSent(sentAt);

        ticket.Status.Should().Be(TicketStatus.Sent);
        ticket.SentAt.Should().Be(sentAt);
    }

    [Fact]
    public void MarkSent_FromOpen_ShouldThrow()
    {
        var ticket = Ticket.Create("Question?", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);

        var act = () => ticket.MarkSent(DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only Drafted tickets can be Sent.");
    }
}
