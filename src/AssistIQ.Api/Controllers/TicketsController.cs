using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthorizationPolicies.TicketsManage)]
public sealed class TicketsController(TicketService service) : ControllerBase
{
    /// <summary>
    /// Creates a new customer support ticket.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.CreateAsync(request, cancellationToken));
    }

    /// <summary>
    /// Lists paginated tickets with their summaries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketSummaryDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPagedAsync(pagination, cancellationToken));
    }

    /// <summary>
    /// Gets detailed information for a specific ticket, including its draft history.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await service.GetAsync(id, cancellationToken));
    }
}

