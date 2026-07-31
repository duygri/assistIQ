using AssistIQ.Api.Auth;
using AssistIQ.Application.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminStatsView)]
public sealed class AnalyticsController(AnalyticsService service) : ControllerBase
{
    /// <summary>
    /// Returns aggregate statistics for the admin dashboard:
    /// ticket and draft counts, token consumption, cost, and the top cited knowledge documents.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAdminStatsAsync(cancellationToken));
    }
}
