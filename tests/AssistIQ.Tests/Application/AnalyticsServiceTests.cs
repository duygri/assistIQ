using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Analytics;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Domain.Tickets;
using AssistIQ.Domain.Usage;
using AssistIQ.Domain.Users;
using AssistIQ.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Tests.Application;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public async Task GetAdminStatsAsync_WithNoData_ShouldReturnZeros()
    {
        await using var scope = await AnalyticsTestScope.CreateAsync();

        var stats = await scope.Service.GetAdminStatsAsync(CancellationToken.None);

        stats.TotalTickets.Should().Be(0);
        stats.TotalDrafts.Should().Be(0);
        stats.SentDrafts.Should().Be(0);
        stats.TotalKnowledgeDocuments.Should().Be(0);
        stats.ReadyKnowledgeDocuments.Should().Be(0);
        stats.TotalTokensUsed.Should().Be(0);
        stats.TotalEstimatedCost.Should().Be(0);
        stats.AverageTokensPerDraft.Should().Be(0);
        stats.TopCitedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAdminStatsAsync_WithData_ShouldReturnCorrectAggregates()
    {
        await using var scope = await AnalyticsTestScope.CreateAsync();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        // Seed tickets
        var ticket = Ticket.Create("Question?", "John", "john@test.com", userId, now);
        scope.DbContext.Tickets.Add(ticket);

        // Seed knowledge documents
        var doc = KnowledgeDocument.CreateIndexing("faq.md", "text/markdown", 256, "FAQ content", userId, now);
        doc.MarkReady("vs-1", "file-1", now);
        scope.DbContext.KnowledgeDocuments.Add(doc);

        // Seed drafts with citations
        var citation = DraftCitation.Create(doc.Id, "faq.md", "file-1", "Quote text", "result-1", 0.95m);
        var draft = Draft.CreateAiGenerated(ticket.Id, 1, "Generated answer", [citation]);
        scope.DbContext.Drafts.Add(draft);

        // Seed usage logs
        var usage = UsageLog.Succeeded(userId, ticket.Id, draft.Id, "Fake", "model", "resp-1", 100, 50, 0.5m, now);
        scope.DbContext.UsageLogs.Add(usage);

        await scope.DbContext.SaveChangesAsync();

        var stats = await scope.Service.GetAdminStatsAsync(CancellationToken.None);

        stats.TotalTickets.Should().Be(1);
        stats.TotalDrafts.Should().Be(1);
        stats.TotalKnowledgeDocuments.Should().Be(1);
        stats.ReadyKnowledgeDocuments.Should().Be(1);
        stats.TotalTokensUsed.Should().Be(150);
        stats.TotalEstimatedCost.Should().Be(0.5m);
        stats.AverageTokensPerDraft.Should().Be(150);
    }

    private sealed class AnalyticsTestScope : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private AnalyticsTestScope(
            SqliteConnection connection,
            AssistIQDbContext dbContext,
            AnalyticsService service)
        {
            _connection = connection;
            DbContext = dbContext;
            Service = service;
        }

        public AssistIQDbContext DbContext { get; }
        public AnalyticsService Service { get; }

        public static async Task<AnalyticsTestScope> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AssistIQDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AssistIQDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var repository = new AnalyticsRepository(dbContext);
            var service = new AnalyticsService(repository);

            return new AnalyticsTestScope(connection, dbContext, service);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
