using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Contracts.Sales;
using Sad.Api.Security;
using Sad.Api.Services.Sales;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController : ControllerBase
{
  private readonly ISalesService _sales;

  public SalesController(ISalesService sales)
  {
    _sales = sales;
  }

  [HttpPost]
  public async Task<ActionResult<SaleDto>> Create(
      [FromBody] SaleCreateDto dto,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(timeZone))
        return BadRequest("timeZone is required.");

      if (dto.StoreId <= 0)
        return BadRequest("StoreId is required.");

      if (dto.Subtotal < 0 || dto.Tax < 0)
        return BadRequest("Subtotal/Tax must be >= 0.");

      var userId = User.GetUserIdOrThrow();

      var created = await _sales.CreateAsync(
          userId,
          dto,
          timeZone,
          ct);

      return CreatedAtAction(
          nameof(GetById),
          new
          {
            saleId = created.SaleId,
            timeZone
          },
          created);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (Exception ex)
    {
      return BadRequest(ex.Message);
    }
  }

  [HttpGet("latest")]
  public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetLatest(
      [FromQuery] int top,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      return BadRequest("timeZone is required.");

    if (top <= 0)
      top = 10;

    try
    {
      var userId = User.GetUserIdOrThrow();

      var latest = await _sales.GetLatestAsync(
          top,
          timeZone,
          ct,
          userId);

      return Ok(latest);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
  }

  [HttpGet("{saleId:guid}")]
  public async Task<ActionResult<SaleDto>> GetById(
      Guid saleId,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      return BadRequest("timeZone is required.");

    try
    {
      var sale = await _sales.GetByIdAsync(
          saleId,
          timeZone,
          ct);

      return sale is null ? NotFound() : Ok(sale);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
  }

  [HttpGet("range")]
  public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndRange(
      [FromQuery] int storeId,
      [FromQuery] DateTime from,
      [FromQuery] DateTime to,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      return BadRequest("timeZone is required.");

    if (storeId <= 0)
      return BadRequest("storeId must be > 0.");

    if (to < from)
      return BadRequest("'to' must be >= 'from'.");

    try
    {
      var list = await _sales.GetByStoreAndRangeAsync(
          storeId,
          from,
          to,
          timeZone,
          ct);

      return Ok(list);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
  }

  [HttpGet("store/{storeId:int}")]
  public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreId(
      int storeId,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      return BadRequest("timeZone is required.");

    if (storeId <= 0)
      return BadRequest("storeId must be > 0.");

    try
    {
      var list = await _sales.GetByStoreIdAsync(
          storeId,
          timeZone,
          ct);

      return Ok(list);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
  }

  [HttpGet("store/{storeId:int}/date/{date:datetime}")]
  public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndDate(
      int storeId,
      DateTime date,
      [FromQuery] string timeZone,
      CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      return BadRequest("timeZone is required.");

    if (storeId <= 0)
      return BadRequest("storeId must be > 0.");

    try
    {
      var list = await _sales.GetByStoreAndDateAsync(
          storeId,
          date,
          timeZone,
          ct);

      return Ok(list);
    }
    catch (TimeZoneNotFoundException)
    {
      return BadRequest("Invalid timeZone.");
    }
    catch (InvalidTimeZoneException)
    {
      return BadRequest("Invalid timeZone.");
    }
  }

  [HttpDelete("{saleId:guid}")]
  public async Task<IActionResult> Delete(Guid saleId, CancellationToken ct)
  {
    var ok = await _sales.DeleteByIdAsync(saleId, ct);
    return ok ? NoContent() : NotFound();
  }
}
