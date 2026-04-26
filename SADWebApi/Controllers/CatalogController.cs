using Microsoft.AspNetCore.Mvc;
using Sad.Api.Services.Catalog;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet("stores")]
    public async Task<IActionResult> GetStores(CancellationToken ct) =>
        Ok(await _catalog.GetStoresAsync(ct));

    [HttpGet("credit-card-products")]
    public async Task<IActionResult> GetCreditCardProducts(CancellationToken ct) =>
        Ok(await _catalog.GetCreditCardProductsAsync(ct));

    [HttpGet("membership-products")]
    public async Task<IActionResult> GetMembershipProducts(CancellationToken ct) =>
        Ok(await _catalog.GetMembershipProductsAsync(ct));

		[HttpGet("sale-status")]
    public async Task<IActionResult> GetSaleStatus(CancellationToken ct) =>
        Ok(await _catalog.GetSaleStatusAsync(ct));
}


//[HttpGet("{id:int}")]
//	public async Task<IActionResult> GetStoreById(int id, CancellationToken ct)
//	{
//		var store = await _catalog.GetStoreByIdAsync(id, ct);

//		if (store == null)
//			return NotFound();

//		return Ok(store);
//	}