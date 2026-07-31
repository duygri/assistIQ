using AssistIQ.Api.Auth;
using AssistIQ.Api.Security;
using AssistIQ.Application.Drafts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DraftsManage)]
public sealed class DraftsController(DraftService service) : ControllerBase
{
    /// <summary>
    /// Generates a new AI draft for a ticket.
    /// </summary>
    [HttpPost("api/tickets/{ticketId:guid}/drafts/generate")]
    [Consumes("application/json")]
    [EnableRateLimiting(ApiRateLimitPolicies.AiDraft)]
    [ProducesResponseType(typeof(DraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DraftDto>> Generate(
        Guid ticketId,
        GenerateDraftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.GenerateAsync(ticketId, request, cancellationToken));
    }

    /// <summary>
    /// Updates the edited answer of an existing draft.
    /// </summary>
    [HttpPatch("api/drafts/{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(DraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DraftDto>> Update(
        Guid id,
        UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Marks a draft as sent, concluding the ticket resolution process.
    /// </summary>
    [HttpPost("api/drafts/{id:guid}/send")]
    [ProducesResponseType(typeof(DraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DraftDto>> Send(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await service.SendAsync(id, cancellationToken));
    }
}
