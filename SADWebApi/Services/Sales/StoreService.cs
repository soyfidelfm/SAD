using Microsoft.EntityFrameworkCore;
using Sad.Api.Data;
using Sad.Api.Data.Entities;

public class StoreService : IStoreService
{
  private readonly SadDbContext _db;

  public StoreService(SadDbContext db)
  {
    _db = db;
  }

  public async Task<int?> GetActiveStoreIdByNumberAsync(string storeNumber, CancellationToken ct)
  {
    if (!int.TryParse(storeNumber, out var storeNum))
      return null;

    return await _db.Stores
      .Where(s => s.StoreNumber == storeNum && s.IsActive)
      .Select(s => (int?)s.StoreId)
      .FirstOrDefaultAsync(ct);
  }

  public async Task<CatalogStore?> GetStoreByIdAsync(int id, CancellationToken ct)
  {
    return await _db.Stores
      .AsNoTracking()
      .Where(x => x.IsActive && x.StoreId == id)
      .FirstOrDefaultAsync(ct);
  }

  public async Task<CatalogStore?> GetStoreByIdIncludingInactiveAsync(int id, CancellationToken ct)
  {
    return await _db.Stores
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.StoreId == id, ct);
  }

  public async Task<IReadOnlyList<CatalogStore>> GetAllStoresAsync(CancellationToken ct)
  {
    return await _db.Stores
      .AsNoTracking()
      .OrderBy(x => x.StoreNumber)
      .ToListAsync(ct);
  }

  public async Task<(CatalogStore? Store, string? Error)> CreateAsync(
    int storeNumber,
    string? storeName,
    bool isActive,
    CancellationToken ct)
  {
    if (storeNumber <= 0)
      return (null, "Store number must be greater than 0.");

    var exists = await _db.Stores.AnyAsync(s => s.StoreNumber == storeNumber, ct);
    if (exists)
      return (null, "A store with that number already exists.");

    var entity = new CatalogStore
    {
      StoreNumber = storeNumber,
      StoreName = NormalizeName(storeName),
      IsActive = isActive
    };

    _db.Stores.Add(entity);
    await _db.SaveChangesAsync(ct);
    return (entity, null);
  }

  public async Task<(bool Found, string? Error)> UpdateAsync(
    int id,
    int storeNumber,
    string? storeName,
    bool isActive,
    CancellationToken ct)
  {
    if (storeNumber <= 0)
      return (true, "Store number must be greater than 0.");

    var entity = await _db.Stores.FirstOrDefaultAsync(x => x.StoreId == id, ct);
    if (entity is null)
      return (false, null);

    var duplicate = await _db.Stores.AnyAsync(
      s => s.StoreNumber == storeNumber && s.StoreId != id,
      ct);

    if (duplicate)
      return (true, "A store with that number already exists.");

    entity.StoreNumber = storeNumber;
    entity.StoreName = NormalizeName(storeName);
    entity.IsActive = isActive;

    await _db.SaveChangesAsync(ct);
    return (true, null);
  }

  public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
  {
    var entity = await _db.Stores.FirstOrDefaultAsync(x => x.StoreId == id, ct);
    if (entity is null)
      return false;

    entity.IsActive = false;
    await _db.SaveChangesAsync(ct);
    return true;
  }

  private static string? NormalizeName(string? name)
  {
    var trimmed = name?.Trim();
    return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
  }
}
