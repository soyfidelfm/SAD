using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SADWebApi.Contracts.Stores;

namespace Sad.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stores")]
public class StoresController : ControllerBase
{
  private readonly IStoreService _stores;

  public StoresController(IStoreService stores) => _stores = stores;

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<StoreDto>>> GetAll(CancellationToken ct)
  {
    var stores = await _stores.GetAllStoresAsync(ct);
    return Ok(stores.Select(ToDto).ToList());
  }

  [HttpGet("{id:int}")]
  public async Task<ActionResult<StoreDto>> GetById(int id, CancellationToken ct)
  {
    var store = await _stores.GetStoreByIdIncludingInactiveAsync(id, ct);
    if (store is null) return NotFound();
    return Ok(ToDto(store));
  }

  [HttpPost]
  public async Task<ActionResult<StoreDto>> Create([FromBody] UpsertStoreDto dto, CancellationToken ct)
  {
    var (store, error) = await _stores.CreateAsync(dto.StoreNumber, dto.StoreName, dto.IsActive, ct);
    if (error is not null)
      return Conflict(new { message = error });

    return Ok(ToDto(store!));
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> Update(int id, [FromBody] UpsertStoreDto dto, CancellationToken ct)
  {
    var (found, error) = await _stores.UpdateAsync(id, dto.StoreNumber, dto.StoreName, dto.IsActive, ct);
    if (!found) return NotFound();
    if (error is not null) return Conflict(new { message = error });
    return NoContent();
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
  {
    var found = await _stores.DeactivateAsync(id, ct);
    if (!found) return NotFound();
    return NoContent();
  }

  private static StoreDto ToDto(Sad.Api.Data.Entities.CatalogStore store) => new()
  {
    StoreId = store.StoreId,
    StoreNumber = store.StoreNumber,
    StoreName = store.StoreName,
    IsActive = store.IsActive
  };
}
