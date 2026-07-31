using System.Net;
using System.Net.Http.Json;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Knowledge;
using AssistIQ.Application.Tickets;
using AssistIQ.Application.Drafts;
using AssistIQ.Application.UsageLogs;
using AssistIQ.Domain.Usage;
using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class AdminLogApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AuditLogs_AsSupportAgent_ShouldReturnForbidden()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");

        var response = await client.GetAsync("/api/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuditLogs_AsAdmin_ShouldReturnOk()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "admin@assistiq.local", "Admin123!");

        var response = await client.GetAsync("/api/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UsageLogs_AsSupportAgent_ShouldReturnForbidden()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");

        var response = await client.GetAsync("/api/usage-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UsageLogs_AsAdmin_ShouldIncludeModelTokensCostAndStatus()
    {
        var client = factory.CreateClient();
        await GenerateDraftAsync(client);
        await AuthenticateAsync(client, "admin@assistiq.local", "Admin123!");

        var response = await client.GetAsync("/api/usage-logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await response.Content.ReadFromJsonAsync<IReadOnlyList<UsageLogDto>>();
        logs.Should().NotBeNull();
        logs.Should().ContainSingle();
        var log = logs![0];
        log.Model.Should().Be("fake-support-copilot-v1");
        log.PromptTokens.Should().BeGreaterThan(0);
        log.CompletionTokens.Should().BeGreaterThan(0);
        log.EstimatedCost.Should().BeGreaterThan(0);
        log.Status.Should().Be(UsageStatus.Succeeded);
    }

    private static async Task GenerateDraftAsync(HttpClient client)
    {
        await AuthenticateAsync(client, "admin@assistiq.local", "Admin123!");
        var knowledge = await client.PostAsJsonAsync("/api/knowledge-documents", new RegisterKnowledgeDocumentRequest(
            "billing.md",
            "text/markdown",
            512,
            "Billing details can be updated from workspace settings."));
        knowledge.EnsureSuccessStatusCode();

        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");
        var ticketResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest(
            "How do I update billing details?",
            "Linh",
            "linh@example.com"));
        ticketResponse.EnsureSuccessStatusCode();
        var ticket = await ticketResponse.Content.ReadFromJsonAsync<TicketDto>();

        var draftResponse = await client.PostAsJsonAsync($"/api/tickets/{ticket!.Id}/drafts/generate", new GenerateDraftRequest(null));
        draftResponse.EnsureSuccessStatusCode();
    }

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);
    }
}
