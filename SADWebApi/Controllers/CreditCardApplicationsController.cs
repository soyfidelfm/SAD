using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data.Entities.Sales;
using Sad.Api.Services.Sales;
using SADWebApi.Contracts.Sales;
using System.Security.Claims;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/credit-card-applications")]
[Authorize] // 🔐 obligatorio
public class CreditCardApplicationsController : ControllerBase
{
	private readonly ICreditCardApplicationsService _service;

	public CreditCardApplicationsController(ICreditCardApplicationsService service)
		=> _service = service;

	[HttpPost]
	public async Task<ActionResult<object>> Create(
		[FromBody] CreateCreditCardApplicationDto dto,
		CancellationToken ct)
	{
		// ✅ PASO 2: obtener UserId desde el JWT
		var uid = User.FindFirstValue("uid");
		if (string.IsNullOrWhiteSpace(uid) || !Guid.TryParse(uid, out var userId))
			return Unauthorized("Missing uid claim.");

		var id = await _service.CreateAsync(userId, dto, ct);

		return CreatedAtAction(
			nameof(GetLatest),
			new { top = 10 },
			new { creditCardApplicationId = id }
		);
	}

	[HttpGet("latest")]
	public async Task<ActionResult<IReadOnlyList<CreditCardApplicationDto>>> GetLatest(
		[FromQuery] int top = 50,
		CancellationToken ct = default)
	{
		top = Math.Clamp(top, 1, 500);
		var items = await _service.GetLatestAsync(top, ct);
		return Ok(items);
	}

	//[HttpGet("{saleId:guid}")]
	//public async Task<ActionResult<SaleDto>> GetById(Guid saleId, CancellationToken ct)
	//{
	//	var sale = await _service.GetByIdAsync(saleId, ct);
	//	return sale is null ? NotFound() : Ok(sale);
	//}

	//// ✅ NUEVO: RANGO por storeId + from + to (1 sola llamada)
	//// GET: /api/sales/range?storeId=1&from=2026-01-01T00:00:00&to=2026-01-06T23:59:59
	//[HttpGet("range")]
	//public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndRange(
	//	[FromQuery] int storeId,
	//	[FromQuery] DateTime from,
	//	[FromQuery] DateTime to,
	//	CancellationToken ct)
	//{
	//	if (storeId <= 0) return BadRequest("storeId must be > 0.");
	//	if (to < from) return BadRequest("'to' must be >= 'from'.");

	//	var list = await _service.GetByStoreAndRangeAsync(storeId, from, to, ct);
	//	return Ok(list);
	//}

	//// ✅ (opcional) mantener esto si lo usas en otros lados
	//[HttpGet("store/{storeId:int}")]
	//public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreId(int storeId, CancellationToken ct)
	//{
	//	if (storeId <= 0) return BadRequest("storeId must be > 0.");
	//	var list = await _sales.GetByStoreIdAsync(storeId, ct);
	//	return Ok(list);
	//}

	//// ✅ (opcional) mantener endpoint diario
	//[HttpGet("store/{storeId:int}/date/{date:datetime}")]
	//public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetByStoreAndDate(int storeId, DateTime date, CancellationToken ct)
	//{
	//	if (storeId <= 0) return BadRequest("storeId must be > 0.");

	//	var list = await _sales.GetByStoreAndDateAsync(storeId, date, ct);
	//	return Ok(list);
	//}

	//[HttpDelete("{saleId:guid}")]
	//public async Task<IActionResult> Delete(Guid saleId, CancellationToken ct)
	//{
	//	var ok = await _sales.DeleteByIdAsync(saleId, ct);
	//	return ok ? NoContent() : NotFound();

	//}
}
