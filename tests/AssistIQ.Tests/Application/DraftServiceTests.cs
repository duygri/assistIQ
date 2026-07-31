using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.Drafts;
using AssistIQ.Application.Tickets;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Tickets;
using AssistIQ.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AssistIQ.Tests.Application;

public sealed class DraftServiceTests
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdateEditedAnswer()
    {
        var tickets = Substitute.For<ITicketRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var ticketService = new TicketService(
            tickets, 
            Substitute.For<IDraftRepository>(),
            Substitute.For<IAuditService>(), 
            currentUser, 
            Substitute.For<ISystemClock>());
            
        var drafts = Substitute.For<IDraftRepository>();
        var retrievalService = Substitute.For<IRetrievalService>();
        var aiDraftService = Substitute.For<IAiDraftService>();
        var usageRecorder = Substitute.For<IUsageRecorder>();
        var auditService = Substitute.For<IAuditService>();

        var draftId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        
        currentUser.UserId.Returns(Guid.NewGuid());
        currentUser.Role.Returns(UserRole.Admin);

        var ticket = Ticket.Create("Q?", null, null, currentUser.UserId, DateTimeOffset.UtcNow);
        tickets.FindByIdAsync(ticketId, Arg.Any<CancellationToken>()).Returns(ticket);
        
        var draft = Draft.CreateAiGenerated(ticketId, 1, "Ans", new List<DraftCitation>());
        
        drafts.FindByIdAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);
        
        var service = new DraftService(ticketService, drafts, retrievalService, aiDraftService, usageRecorder, auditService, currentUser);

        var result = await service.UpdateAsync(draft.Id, new UpdateDraftRequest("New Ans"), CancellationToken.None);

        result.EditedAnswer.Should().Be("New Ans");
        result.Status.Should().Be(DraftStatus.Edited);
    }
}
