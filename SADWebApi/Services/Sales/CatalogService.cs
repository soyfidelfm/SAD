using Microsoft.EntityFrameworkCore;
using Sad.Api.Data;
using Sad.Api.Data.Entities;

namespace Sad.Api.Services.Catalog;

public class CatalogService : ICatalogService
{
    private readonly SadDbContext _db;
    public CatalogService(SadDbContext db) => _db = db;

    public async Task<IReadOnlyList<CatalogStore>> GetStoresAsync(CancellationToken ct) =>
        await _db.Stores.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.StoreNumber).ToListAsync(ct);


	public async Task<IReadOnlyList<CatalogCreditCardProduct>> GetCreditCardProductsAsync(CancellationToken ct) =>
        await _db.CreditCardProducts.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ProductName).ToListAsync(ct);

    public async Task<IReadOnlyList<CatalogMembershipProduct>> GetMembershipProductsAsync(CancellationToken ct) =>
        await _db.MembershipProducts.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ProductName).ToListAsync(ct);

    public async Task<IReadOnlyList<CatalogSaleStatus>> GetSaleStatusAsync(CancellationToken ct) =>
        await _db.SaleStatus.AsNoTracking().OrderBy(x => x.StatusId).ToListAsync(ct);
}
