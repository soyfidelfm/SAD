using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Contracts.Sales;
using Sad.Api.Services.Sales;
using SADWebApi.Contracts.Sales;
using System.Security.Claims;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/credit-card-applications")]
[Authorize]
public class CreditCardApplicationsController : ControllerBase
{
  private readonly ICreditCardApplicationsService _service;

  public CreditCardApplicationsController(ICreditCardApplicationsService service)
  {
    _service = service;
  }

  [HttpPost]
  public async Task<ActionResult<object>> Create(
    [FromBody] CreateCreditCardApplicationDto dto,
    [FromQuery] string timeZone = "UTC",
    CancellationToken ct = default)
  {
    var uid = User.FindFirstValue("uid");

    if (string.IsNullOrWhiteSpace(uid) || !Guid.TryParse(uid, out var userId))
      return Unauthorized("Missing uid claim.");

    var id = await _service.CreateAsync(
      userId,
      dto,
      timeZone,
      ct);

    return CreatedAtAction(
      nameof(GetLatest),
      new
      {
        top = 10,
        timeZone
      },
      new
      {
        creditCardApplicationId = id
      }
    );
  }

  [HttpGet("latest")]
  public async Task<ActionResult<IReadOnlyList<CreditCardApplicationDto>>> GetLatest(
    [FromQuery] int top = 50,
    [FromQuery] string timeZone = "UTC",
    CancellationToken ct = default)
  {
    top = Math.Clamp(top, 1, 500);

    var items = await _service.GetLatestAsync(
      top,
      timeZone,
      ct);

    return Ok(items);
  }
}
