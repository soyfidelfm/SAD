using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Contracts.Auth;
using Sad.Api.Services.Auth;
using System.Security.Claims;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // POST: api/auth/external-login
    [HttpPost("external-login")]
    public async Task<ActionResult<UserIdResponseDto>> ExternalLogin(
        [FromBody] ExternalLoginRequestDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.IdentityProviderCode) || string.IsNullOrWhiteSpace(dto.ProviderSubject))
            return BadRequest("IdentityProviderCode and ProviderSubject are required.");

        var userId = await _auth.UpsertUserFromExternalLoginAsync(dto, ct);
        return Ok(new UserIdResponseDto(userId));
    }
	[Authorize]
	[HttpGet("me")]
	public IActionResult Me()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var aNumber = User.FindFirst("aNumber")?.Value;
		var storeId = User.FindFirst("storeId")?.Value;
		var storeName = User.FindFirst("storeName")?.Value;
		var role = User.FindFirst(ClaimTypes.Role)?.Value;

		return Ok(new
		{
			userId,
			aNumber,
			storeId,
			storeName,
			role
		});
	}

}
