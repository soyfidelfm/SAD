using Sad.Api.Data.Entities;

public interface IStoreService
{
	Task<int?> GetActiveStoreIdByNumberAsync(string storeNumber, CancellationToken ct);
	Task<CatalogStore?> GetStoreByIdAsync(int id, CancellationToken ct);
}
