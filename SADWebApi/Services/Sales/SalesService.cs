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
    var nowUtc = DateTime.UtcNow;

    var saleDateUtc = dto.SaleDate.HasValue
        ? DateTimeHelper.ConvertPstToUtc(dto.SaleDate.Value)
        : nowUtc;

    var sale = new Sale
    {
      StoreId = dto.StoreId,
      UserId = userId,
      SaleDate = saleDateUtc,
      Subtotal = dto.Subtotal,
      Tax = dto.Tax,
      Total = dto.Subtotal + dto.Tax,
      PaymentMethod = dto.PaymentMethod,
      Notes = dto.Notes,
      CreatedAt = nowUtc,
      UpdatedAt = nowUtc,
      StatusId = 4 // Assuming 1 is the default status for a new sale
    };

    _db.Sales.Add(sale);
    await _db.SaveChangesAsync(ct);

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
    var q = _db.Sales
        .AsNoTracking()
        .AsQueryable();

    if (storeId.HasValue)
      q = q.Where(x => x.StoreId == storeId.Value);

    if (userId.HasValue)
      q = q.Where(x => x.UserId == userId.Value);

    if (fromLocal.HasValue)
    {
      var fromUtc = DateTimeHelper.ConvertPstToUtc(fromLocal.Value);
      q = q.Where(x => x.SaleDate >= fromUtc);
    }

    if (toLocal.HasValue)
    {
      var toUtc = DateTimeHelper.ConvertPstToUtc(toLocal.Value);
      q = q.Where(x => x.SaleDate < toUtc);
    }

    return await q
        .OrderByDescending(x => x.SaleDate)
        .Select(x => ToDto(x))
        .ToListAsync(ct);
  }

  public async Task<bool> UpdateAsync(Guid saleId, SaleUpdateDto dto, CancellationToken ct)
  {
    var sale = await _db.Sales
        .FirstOrDefaultAsync(x => x.SaleId == saleId, ct);

    if (sale is null)
      return false;

    if (dto.SaleDate.HasValue)
      sale.SaleDate = DateTimeHelper.ConvertPstToUtc(dto.SaleDate.Value);

    sale.Subtotal = dto.Subtotal;
    sale.Tax = dto.Tax;
    sale.Total = dto.Subtotal + dto.Tax;
    sale.PaymentMethod = dto.PaymentMethod;
    sale.Notes = dto.Notes;
    sale.UpdatedAt = DateTime.UtcNow;

    await _db.SaveChangesAsync(ct);
    return true;
  }

  public async Task<bool> DeleteAsync(Guid saleId, CancellationToken ct)
  {
    var sale = await _db.Sales
        .FirstOrDefaultAsync(x => x.SaleId == saleId, ct);

    if (sale is null)
      return false;

    _db.Sales.Remove(sale);
    await _db.SaveChangesAsync(ct);

    return true;
  }

  public Task<IReadOnlyList<SaleDto>> GetByStoreIdAsync(int storeId, CancellationToken ct)
  {
    return GetAsync(storeId, userId: null, fromLocal: null, toLocal: null, ct);
  }

  public Task<IReadOnlyList<SaleDto>> GetByStoreAndDateAsync(int storeId, DateTime date, CancellationToken ct)
  {
    var fromLocal = date.Date;
    var toLocal = date.Date.AddDays(1);

    return GetAsync(storeId, userId: null, fromLocal: fromLocal, toLocal: toLocal, ct);
  }

  public Task<IReadOnlyList<SaleDto>> GetByStoreAndRangeAsync(
      int storeId,
      DateTime from,
      DateTime to,
      CancellationToken ct)
  {
    return GetAsync(storeId, userId: null, fromLocal: from, toLocal: to, ct);
  }

  public Task<bool> DeleteByIdAsync(Guid saleId, CancellationToken ct)
  {
    return DeleteAsync(saleId, ct);
  }

  public async Task<IReadOnlyList<SaleDto>> GetLatestAsync(
      int top,
      CancellationToken ct,
      Guid? userId = null)
  {
    return await _db.Sales
        .AsNoTracking()
        .Where(x => !userId.HasValue || x.UserId == userId.Value)
        .OrderByDescending(x => x.SaleDate)
        .Take(top)
        .Select(x => ToDto(x))
        .ToListAsync(ct);
  }

  public async Task<SalesSummaryDto> GetSummaryAsync(Guid userId,
      DateOnly date,
      string timeZone,
      CancellationToken ct)
      {
    // Fallback por si no mandan timezone
    var tz = string.IsNullOrWhiteSpace(timeZone)
        ? TimeZoneInfo.Utc
        : TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    // Convertir el día LOCAL a rango UTC
    var startLocal = date.ToDateTime(TimeOnly.MinValue);
    var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

    var query = _db.Sales
        .AsNoTracking()
        .Where(x => x.UserId == userId);
    var total = await query.CountAsync(ct);
    var today = await query.CountAsync(x => x.SaleDate >= startUtc && x.SaleDate < endUtc, ct);

    var monthStartLocal = new DateTime(date.Year, date.Month, 1);
    var monthEndLocal = monthStartLocal.AddMonths(1);

    var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(monthStartLocal, tz);
    var monthEndUtc = TimeZoneInfo.ConvertTimeToUtc(monthEndLocal, tz);

    var thisMonth = await query.CountAsync(x => x.SaleDate >= monthStartUtc && x.SaleDate < monthEndUtc, ct);

    return new SalesSummaryDto(
    total,
    today,
    thisMonth
    );
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
}
