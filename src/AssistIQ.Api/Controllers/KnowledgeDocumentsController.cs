using AssistIQ.Api.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Knowledge;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/knowledge-documents")]
[Authorize(Policy = AuthorizationPolicies.KnowledgeManage)]
public sealed class KnowledgeDocumentsController(KnowledgeDocumentService service) : ControllerBase
{
    /// <summary>
    /// Lists paginated knowledge documents.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<KnowledgeDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<KnowledgeDocumentDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListPagedAsync(pagination, cancellationToken));
    }

    /// <summary>
    /// Registers a new knowledge document.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(KnowledgeDocumentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KnowledgeDocumentDto>> Register(
        RegisterKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await service.RegisterAsync(request, cancellationToken));
    }

    /// <summary>
    /// Disables an existing knowledge document, preventing it from being cited in future drafts.
    /// </summary>
    [HttpPost("{id:guid}/disable")]
    [ProducesResponseType(typeof(KnowledgeDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KnowledgeDocumentDto>> Disable(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await service.DisableAsync(id, cancellationToken));
    }
}

