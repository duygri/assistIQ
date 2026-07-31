using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.UsageLogs;
using AssistIQ.Domain.Usage;
using FluentAssertions;
using NSubstitute;

namespace AssistIQ.Tests.Application;

public sealed class UsageLogQueryServiceTests
{
    [Fact]
    public async Task ListPagedAsync_ShouldReturnPagedResult()
    {
        var repository = Substitute.For<IUsageLogRepository>();
        var service = new UsageLogQueryService(repository);

        var log = UsageLog.Succeeded(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fake", "Model", "Resp1", 10, 10, 0.1m, DateTimeOffset.UtcNow);
        repository.ListPagedAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns((new List<UsageLog> { log }, 1));

        var result = await service.ListPagedAsync(new PaginationRequest { Page = 1, PageSize = 10 }, CancellationToken.None);

        result.Total.Should().Be(1);
        result.Data.Should().ContainSingle();
        result.Data[0].Model.Should().Be("Model");
    }
}
