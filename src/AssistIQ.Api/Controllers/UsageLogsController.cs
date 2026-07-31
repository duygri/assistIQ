using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.UsageLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/usage-logs")]
[Authorize(Policy = AuthorizationPolicies.UsageLogsView)]
public sealed class UsageLogsController(UsageLogQueryService service) : ControllerBase
{
    /// <summary>
    /// Lists paginated AI usage logs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UsageLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UsageLogDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPagedAsync(pagination, cancellationToken));
    }
}

