using AssistIQ.Api.Auth;
using AssistIQ.Application.AuditLogs;
using AssistIQ.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.AuditLogsView)]
public sealed class AuditLogsController(AuditLogQueryService service) : ControllerBase
{
    /// <summary>
    /// Lists paginated audit logs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPagedAsync(pagination, cancellationToken));
    }
}

