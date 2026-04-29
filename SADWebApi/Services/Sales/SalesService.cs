using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities.Sales;
using SADWebApi.Contracts.Helpers;

namespace Sad.Api.Services.Sales;

public class SalesService : ISalesService
{
	private readonly SadDbContext _db;

	public SalesService(SadDbContext db) => _db = db;

	public async Task<SaleDto> CreateAsync(Guid userId, SaleCreateDto dto, CancellationToken ct)
	{
		var saleDateUtc = dto.SaleDate.HasValue
			? DateTimeHelper.ConvertPstToUtc(dto.SaleDate.Value) // si viene PST
			: DateTime.UtcNow;

		var sale = new Sale
		{
			StoreId = dto.StoreId,
			UserId = userId, // ✅ desde token
			SaleDate = saleDateUtc, // ✅ guardado UTC
			Subtotal = dto.Subtotal,
			Tax = dto.Tax,
			PaymentMethod = dto.PaymentMethod,
			Notes = dto.Notes,
			CreatedAt = DateTime.UtcNow
		};

		_db.Sales.Add(sale);
		await _db.SaveChangesAsync(ct);

		await _db.Entry(sale).ReloadAsync(ct);
		return ToDto(sale);
	}

	public async Task<SaleDto?> GetByIdAsync(Guid saleId, CancellationToken ct)
	{
		var sale = await _db.Sales
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.SaleId == saleId, ct);

		return sale is null ? null : ToDto(sale);
	}
  public async Task<IReadOnlyList<SaleDto>> GetAsync(
		int? storeId,
		Guid? userId,
		DateTime? fromLocal,
		DateTime? toLocal,
		CancellationToken ct)
	{
		var q = _db.Sales.AsNoTracking().AsQueryable();

		if (storeId.HasValue)
			q = q.Where(x => x.StoreId == storeId.Value);

		if (userId.HasValue)
			q = q.Where(x => x.UserId == userId.Value);

		// ✅ PST → UTC (solo si vienen filtros)
		if (fromLocal.HasValue)
		{
			var fromUtc = DateTimeHelper.ConvertPstToUtc(fromLocal.Value);
			q = q.Where(x => x.SaleDate >= fromUtc);
		}

		if (toLocal.HasValue)
		{
			var toUtc = DateTimeHelper.ConvertPstToUtc(toLocal.Value);
			// ✅ rango half-open: < end
			q = q.Where(x => x.SaleDate < toUtc);
		}

		return await q
			.OrderByDescending(x => x.SaleDate)
			.Select(x => ToDto(x))
			.ToListAsync(ct);
	}

	public async Task<bool> UpdateAsync(Guid saleId, SaleUpdateDto dto, CancellationToken ct)
	{
		var sale = await _db.Sales.FirstOrDefaultAsync(x => x.SaleId == saleId, ct);
		if (sale is null) return false;

		if (dto.SaleDate.HasValue)
			sale.SaleDate = DateTimeHelper.ConvertPstToUtc(dto.SaleDate.Value); // ✅ guardar UTC

		sale.Subtotal = dto.Subtotal;
		sale.Tax = dto.Tax;
		sale.PaymentMethod = dto.PaymentMethod;
		sale.Notes = dto.Notes;
		sale.UpdatedAt = DateTime.UtcNow; // ✅ consistente

		await _db.SaveChangesAsync(ct);
		return true;
	}

	public async Task<bool> DeleteAsync(Guid saleId, CancellationToken ct)
	{
		var sale = await _db.Sales.FirstOrDefaultAsync(x => x.SaleId == saleId, ct);
		if (sale is null) return false;

		_db.Sales.Remove(sale);
		await _db.SaveChangesAsync(ct);
		return true;
	}

	// ✅ Todos por StoreId
	public Task<IReadOnlyList<SaleDto>> GetByStoreIdAsync(int storeId, CancellationToken ct)
	{
		return GetAsync(storeId, userId: null, fromLocal: null, toLocal: null, ct);
	}

	// ✅ Por fecha (día PST) y StoreId
	public Task<IReadOnlyList<SaleDto>> GetByStoreAndDateAsync(int storeId, DateTime date, CancellationToken ct)
	{
		// date = día en PST (solo fecha)
		var fromLocal = date.Date;          // 00:00 PST
		var toLocal = date.Date.AddDays(1); // 00:00 PST next day (exclusivo)

		return GetAsync(storeId, userId: null, fromLocal: fromLocal, toLocal: toLocal, ct);
	}

	// ✅ Rango (PST) — 1 sola llamada
	public Task<IReadOnlyList<SaleDto>> GetByStoreAndRangeAsync(int storeId, DateTime from, DateTime to, CancellationToken ct)
	{
		// from/to se asumen PST
		return GetAsync(storeId, userId: null, fromLocal: from, toLocal: to, ct);
	}

	// ✅ Borrar por Id (alias)
	public Task<bool> DeleteByIdAsync(Guid saleId, CancellationToken ct)
	{
		return DeleteAsync(saleId, ct);
	}

	private static SaleDto ToDto(Sale s) =>
		new(
			s.SaleId,
			s.StoreId,
			s.UserId,
			s.SaleDate,
			s.Subtotal,
			s.Tax,
			s.Total,
			s.PaymentMethod,
			s.Notes,
			s.CreatedAt,
			s.UpdatedAt
		);

  public async Task<IReadOnlyList<SaleDto>> GetLatestAsync(int top, CancellationToken ct,  Guid? userId = null)
  {
    return await _db.Sales
      .AsNoTracking()
      .Where(x => !userId.HasValue || x.UserId == userId.Value)
      .OrderByDescending(x => x.SaleDate)
      .Take(top)
      .Select(x => ToDto(x))
      .ToListAsync(ct);
  }
}
