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
	public SalesController(ISalesService sales) => _sales = sales;

  [HttpPost]
  public async Task<ActionResult<SaleDto>> Create([FromBody] SaleCreateDto dto, CancellationToken ct)
  {
    try
    {
      if (dto.StoreId <= 0) return BadRequest("StoreId is required.");
      if (dto.Subtotal < 0 || dto.Tax < 0) return BadRequest("Subtotal/Tax must be >= 0.");

      var userId = User.GetUserIdOrThrow();
      var created = await _sales.CreateAsync(userId, dto, ct);

      return CreatedAtAction(nameof(GetById), new { saleId = created.SaleId }, created);
    }
    catch (Exception ex)
    {
      return BadRequest(ex.Message);
    }
	}

  [HttpGet("latest")]
  public async Task<ActionResult<SaleDto>> GetLatest(int top, CancellationToken ct)
  {
    var userId = User.GetUserIdOrThrow();
    var latest = await _sales.GetLatestAsync(top, ct, userId);
    return latest is null ? NotFound() : Ok(latest);
  }

  [HttpGet("{saleId:guid}")]
	public async Task<ActionResult<SaleDto>> GetById(Guid saleId, CancellationToken ct)
	{
		var sale = await _sales.GetByIdAsync(saleId, ct);
		return sale is null ? NotFound() : Ok(sale);
	}

	// ✅ NUEVO: RANGO por storeId + from + to (1 sola llamada)
	// GET: /api/sales/range?storeId=1&from=2026-01-01T00:00:00&to=2026-01-06T23:59:59
	[HttpGet("range")]
	public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndRange(
		[FromQuery] int storeId,
		[FromQuery] DateTime from,
		[FromQuery] DateTime to,
		CancellationToken ct)
	{
		if (storeId <= 0) return BadRequest("storeId must be > 0.");
		if (to < from) return BadRequest("'to' must be >= 'from'.");

		var list = await _sales.GetByStoreAndRangeAsync(storeId, from, to, ct);
		return Ok(list);
	}

	// ✅ (opcional) mantener esto si lo usas en otros lados
	[HttpGet("store/{storeId:int}")]
	public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreId(int storeId, CancellationToken ct)
	{
		if (storeId <= 0) return BadRequest("storeId must be > 0.");
		var list = await _sales.GetByStoreIdAsync(storeId, ct);
		return Ok(list);
	}

	// ✅ (opcional) mantener endpoint diario
	[HttpGet("store/{storeId:int}/date/{date:datetime}")]
	public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndDate(int storeId, DateTime date, CancellationToken ct)
	{
		if (storeId <= 0) return BadRequest("storeId must be > 0.");

		var list = await _sales.GetByStoreAndDateAsync(storeId, date, ct);
		return Ok(list);
	}

	[HttpDelete("{saleId:guid}")]
	public async Task<IActionResult> Delete(Guid saleId, CancellationToken ct)
	{
		var ok = await _sales.DeleteByIdAsync(saleId, ct);
		return ok ? NoContent() : NotFound();
	}
}
