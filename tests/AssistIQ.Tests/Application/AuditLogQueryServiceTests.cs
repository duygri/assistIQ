using AssistIQ.Application.Abstractions;
using AssistIQ.Application.AuditLogs;
using AssistIQ.Application.Common;
using AssistIQ.Domain.Audit;
using FluentAssertions;
using NSubstitute;

namespace AssistIQ.Tests.Application;

public sealed class AuditLogQueryServiceTests
{
    [Fact]
    public async Task ListPagedAsync_ShouldReturnPagedResult()
    {
        var repository = Substitute.For<IAuditLogRepository>();
        var service = new AuditLogQueryService(repository);

        var log = AuditLog.Create(Guid.NewGuid(), AuditAction.TicketCreated, "Ticket", Guid.NewGuid(), DateTimeOffset.UtcNow);
        repository.ListPagedAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog> { log }, 1));

        var result = await service.ListPagedAsync(new PaginationRequest { Page = 1, PageSize = 10 }, CancellationToken.None);

        result.Total.Should().Be(1);
        result.Data.Should().ContainSingle();
        result.Data[0].Action.Should().Be(AuditAction.TicketCreated);
    }
}
