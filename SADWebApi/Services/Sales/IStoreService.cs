using Sad.Api.Data.Entities;

public interface IStoreService
{
  Task<int?> GetActiveStoreIdByNumberAsync(string storeNumber, CancellationToken ct);
  Task<CatalogStore?> GetStoreByIdAsync(int id, CancellationToken ct);
  Task<CatalogStore?> GetStoreByIdIncludingInactiveAsync(int id, CancellationToken ct);
  Task<IReadOnlyList<CatalogStore>> GetAllStoresAsync(CancellationToken ct);
  Task<(CatalogStore? Store, string? Error)> CreateAsync(int storeNumber, string? storeName, bool isActive, CancellationToken ct);
  Task<(bool Found, string? Error)> UpdateAsync(int id, int storeNumber, string? storeName, bool isActive, CancellationToken ct);
  Task<bool> DeactivateAsync(int id, CancellationToken ct);
}
