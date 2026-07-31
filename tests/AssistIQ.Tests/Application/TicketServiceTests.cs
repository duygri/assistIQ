using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.Tickets;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Tickets;
using AssistIQ.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AssistIQ.Tests.Application;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnTicketDto()
    {
        var repository = Substitute.For<ITicketRepository>();
        var draftRepository = Substitute.For<IDraftRepository>();
        var auditService = Substitute.For<IAuditService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var clock = Substitute.For<ISystemClock>();
        
        currentUser.UserId.Returns(Guid.NewGuid());
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        var service = new TicketService(repository, draftRepository, auditService, currentUser, clock);

        var result = await service.CreateAsync(new CreateTicketRequest("Q?", "Name", "Email"), CancellationToken.None);

        result.CustomerQuestion.Should().Be("Q?");
        await repository.Received(1).AddAsync(Arg.Any<Ticket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WhenUserIsSupportAgent_AndTicketCreatedByAnother_ShouldThrow()
    {
        var repository = Substitute.For<ITicketRepository>();
        var draftRepository = Substitute.For<IDraftRepository>();
        var auditService = Substitute.For<IAuditService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var clock = Substitute.For<ISystemClock>();

        var otherUserId = Guid.NewGuid();
        currentUser.UserId.Returns(Guid.NewGuid());
        currentUser.Role.Returns(UserRole.SupportAgent);
        
        var ticket = Ticket.Create("Q?", null, null, otherUserId, DateTimeOffset.UtcNow);
        repository.FindByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);

        var service = new TicketService(repository, draftRepository, auditService, currentUser, clock);

        var act = () => service.GetAsync(ticket.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(e => e.ErrorCode == ErrorCodes.Unauthorized);
    }
}
