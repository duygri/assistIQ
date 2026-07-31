using AssistIQ.Application.Auth;
using AssistIQ.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [Consumes("application/json")]
    [EnableRateLimiting(ApiRateLimitPolicies.Login)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        return Ok(await authService.GetCurrentUserAsync(cancellationToken));
    }
}
