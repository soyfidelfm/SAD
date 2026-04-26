using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sad.Api.Services.Catalog; // ajusta el namespace de tu IStoreService
using System.Threading;

namespace Sad.Api.Controllers;

[ApiController]
[Route("api/stores")]
public class StoresController : ControllerBase
{
	private readonly IStoreService _stores;

	public StoresController(IStoreService stores) => _stores = stores;

	// GET /api/stores/5
	[Authorize]
	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id, CancellationToken ct)
	{
		var store = await _stores.GetStoreByIdAsync(id, ct);
		if (store is null) return NotFound();
		return Ok(store);
	}

	//// GET /api/stores (para dropdowns)
	//[Authorize]
	//[HttpGet]
	//public async Task<IActionResult> GetActive(CancellationToken ct)
	//{
	//	var stores = await _stores.GetStoresAsync(ct);
	//	return Ok(stores);
	//}
}
