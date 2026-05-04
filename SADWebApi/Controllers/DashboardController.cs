using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Services.Dashboard;
using SADWebApi.Contracts.Dashboard;
using System.Security.Claims;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
	private readonly IDashboardService _dashboard;

	public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

  // GET /api/dashboard/summary?date=2026-05-04&timeZone=America/Los_Angeles
  [HttpGet("summary")]
  public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
      [FromQuery] DateOnly date,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    var userId = GetUserIdFromClaims();

    var summary = await _dashboard.GetDashboardSummaryAsync(
        userId,
        date,
        timeZone,
        ct
    );

    return Ok(summary);
  }

  private Guid GetUserIdFromClaims()
	{
		// usa el claim que tú estés emitiendo en el JWT
		// común: "sub" o ClaimTypes.NameIdentifier
		var raw =
			User.FindFirstValue("sub") ??
			User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
			throw new UnauthorizedAccessException("Missing/invalid user id claim.");

		return userId;
	}

  [HttpGet("latestTransactions")]
  public async Task<ActionResult<IEnumerable<LatestTransactionDto>>> GetLatestTransactions(int top, CancellationToken ct)
  {
    var userId = GetUserIdFromClaims();
    var transactions = await _dashboard.GetLastTransactionsAsync(top, ct, userId);
    return Ok(transactions);
  }

  [HttpGet("today/by-hour")]
	public async Task<ActionResult<IEnumerable<SalesByHourDto>>> GetTodaySalesByHour(
	
	CancellationToken ct)
	{
		var userIdRaw = User.FindFirst("uid")?.Value
					?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		if (string.IsNullOrWhiteSpace(userIdRaw) || !Guid.TryParse(userIdRaw, out var userId))
			return Unauthorized("Invalid or missing user id in token.");
		var result = await _dashboard.GetTodaySalesByHourAsync(userId, ct);

		return Ok(result);
	}
}
