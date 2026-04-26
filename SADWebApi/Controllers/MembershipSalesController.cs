using Microsoft.AspNetCore.Mvc;
using Sad.Api.Contracts.Sales;
using Sad.Api.Services.Sales;
using System.Security.Claims;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/membership-sales")]
public class MembershipSalesController : ControllerBase
{
    private readonly IMembershipSalesService _service;

    public MembershipSalesController(IMembershipSalesService service) => _service = service;

	[HttpPost]
	public async Task<ActionResult<object>> Create(CreateMembershipSaleDto dto, CancellationToken ct)
	{
		var userIdStr =
			User.FindFirstValue("uid") ??
			User.FindFirstValue(ClaimTypes.NameIdentifier) ??
			User.FindFirstValue("sub");

		if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
			return Unauthorized("Missing/invalid user id claim.");

		var id = await _service.CreateAsync(userId, dto, ct);
		return Ok(new { membershipSaleId = id });
	}

	[HttpGet("latest")]
    public async Task<ActionResult<IReadOnlyList<MembershipSaleDto>>> GetLatest([FromQuery] int top = 50, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 500);
        var items = await _service.GetLatestAsync(top, ct);
        return Ok(items);
    }
}
