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
}
