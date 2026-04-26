using Sad.Api.Data.Entities;

namespace Sad.Api.Services.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<CatalogStore>> GetStoresAsync(CancellationToken ct);
    Task<IReadOnlyList<CatalogCreditCardProduct>> GetCreditCardProductsAsync(CancellationToken ct);
    Task<IReadOnlyList<CatalogMembershipProduct>> GetMembershipProductsAsync(CancellationToken ct);
    Task<IReadOnlyList<CatalogSaleStatus>> GetSaleStatusAsync(CancellationToken ct);
	

}
