namespace AssistIQ.Application.Analytics;

/// <summary>
/// Provides aggregate statistics for the admin dashboard.
/// Queries are intentionally kept simple and composable; for large data sets
/// the caller should consider adding caching at the controller or service level.
/// </summary>
public interface IAnalyticsRepository
{
    Task<int> CountTicketsAsync(CancellationToken cancellationToken);

    Task<int> CountDraftsAsync(CancellationToken cancellationToken);

    Task<int> CountSentDraftsAsync(CancellationToken cancellationToken);

    Task<int> CountKnowledgeDocumentsAsync(CancellationToken cancellationToken);

    Task<int> CountReadyKnowledgeDocumentsAsync(CancellationToken cancellationToken);

    Task<(long TotalTokens, decimal TotalCost)> SumTokensAndCostAsync(CancellationToken cancellationToken);

    /// <returns>
    /// List of (KnowledgeDocumentId, FileName, CitationCount), ordered by citation count descending,
    /// limited to <paramref name="topN"/>.
    /// </returns>
    Task<IReadOnlyList<(Guid DocumentId, string FileName, int Count)>> TopCitedDocumentsAsync(
        int topN,
        CancellationToken cancellationToken);
}
