using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities.Sales;

namespace Sad.Api.Services.Sales;

public class SalesService : ISalesService
{
  private readonly SadDbContext _db;

  public SalesService(SadDbContext db) => _db = db;

  public async Task<SaleDto> CreateAsync(
      Guid userId,
      SaleCreateDto dto,
      string timeZone,
      CancellationToken ct)
  {
    var tz = GetTimeZone(timeZone);
    var nowUtc = DateTime.UtcNow;

    var saleDateUtc = dto.SaleDate.HasValue
        ? ConvertLocalToUtc(dto.SaleDate.Value, tz)
        : nowUtc;

    var sale = new Sale
    {
      StoreId = dto.StoreId,
      UserId = userId,
      SaleDate = dto.SaleDate.Value,
      Subtotal = dto.Subtotal,
      Tax = dto.Tax,
      Total = dto.Subtotal + dto.Tax,
      PaymentMethod = dto.PaymentMethod,
      Notes = dto.Notes,
      CreatedAt = nowUtc,
      UpdatedAt = nowUtc,
      StatusId = 4
    };

    _db.Sales.Add(sale);
    await _db.SaveChangesAsync(ct);

    return ToDto(sale, tz);
  }

  public async Task<SaleDto?> GetByIdAsync(
      Guid saleId,
      string timeZone,
      CancellationToken ct)
  {
    var tz = GetTimeZone(timeZone);

    var sale = await _db.Sales
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.SaleId == saleId, ct);

    return sale is null ? null : ToDto(sale, tz);
  }

  public async Task<IReadOnlyList<SaleDto>> GetAsync(
      int? storeId,
      Guid? userId,
      DateTime? fromLocal,
      DateTime? toLocal,
      string timeZone,
      CancellationToken ct)
  {
    var tz = GetTimeZone(timeZone);

    var q = _db.Sales
        .AsNoTracking()
        .AsQueryable();

    if (storeId.HasValue)
      q = q.Where(x => x.StoreId == storeId.Value);

    if (userId.HasValue)
      q = q.Where(x => x.UserId == userId.Value);

    if (fromLocal.HasValue)
    {
      var fromUtc = ConvertLocalToUtc(fromLocal.Value, tz);
      q = q.Where(x => x.SaleDate >= fromUtc);
    }

    if (toLocal.HasValue)
    {
      var toUtc = ConvertLocalToUtc(toLocal.Value, tz);
      q = q.Where(x => x.SaleDate < toUtc);
    }

    var sales = await q
        .OrderByDescending(x => x.SaleDate)
        .ToListAsync(ct);

    return sales
        .Select(x => ToDto(x, tz))
        .ToList();
  }

  public async Task<bool> UpdateAsync(
      Guid saleId,
      SaleUpdateDto dto,
      string timeZone,
      CancellationToken ct)
  {
    var tz = GetTimeZone(timeZone);

    var sale = await _db.Sales
        .FirstOrDefaultAsync(x => x.SaleId == saleId, ct);

    if (sale is null)
      return false;

    if (dto.SaleDate.HasValue)
      sale.SaleDate = ConvertLocalToUtc(dto.SaleDate.Value, tz);

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

  public Task<IReadOnlyList<SaleDto>> GetByStoreIdAsync(
      int storeId,
      string timeZone,
      CancellationToken ct)
  {
    return GetAsync(
        storeId,
        userId: null,
        fromLocal: null,
        toLocal: null,
        timeZone,
        ct);
  }

  public Task<IReadOnlyList<SaleDto>> GetByStoreAndDateAsync(
      int storeId,
      DateTime date,
      string timeZone,
      CancellationToken ct)
  {
    var fromLocal = date.Date;
    var toLocal = date.Date.AddDays(1);

    return GetAsync(
        storeId,
        userId: null,
        fromLocal,
        toLocal,
        timeZone,
        ct);
  }

  public Task<IReadOnlyList<SaleDto>> GetByStoreAndRangeAsync(
      int storeId,
      DateTime from,
      DateTime to,
      string timeZone,
      CancellationToken ct)
  {
    return GetAsync(
        storeId,
        userId: null,
        fromLocal: from,
        toLocal: to,
        timeZone,
        ct);
  }

  public Task<bool> DeleteByIdAsync(Guid saleId, CancellationToken ct)
  {
    return DeleteAsync(saleId, ct);
  }

  public async Task<IReadOnlyList<SaleDto>> GetLatestAsync(
      int top,
      string timeZone,
      CancellationToken ct,
      Guid? userId = null)
  {
    var tz = GetTimeZone(timeZone);

    var sales = await _db.Sales
        .AsNoTracking()
        .Where(x => !userId.HasValue || x.UserId == userId.Value)
        .OrderByDescending(x => x.SaleDate)
        .Take(top)
        .ToListAsync(ct);

    return sales
        .Select(x => ToDto(x, tz))
        .ToList();
  }

  public async Task<SalesSummaryDto> GetSummaryAsync(
      Guid userId,
      DateOnly date,
      string timeZone,
      CancellationToken ct)
  {
    var tz = GetTimeZone(timeZone);

    var startLocal = date.ToDateTime(TimeOnly.MinValue);
    var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

    var startUtc = ConvertLocalToUtc(startLocal, tz);
    var endUtc = ConvertLocalToUtc(endLocal, tz);

    var query = _db.Sales
        .AsNoTracking()
        .Where(x => x.UserId == userId);

    var total = await query.SumAsync(x => x.Subtotal, ct);

    var today = await query
        .Where(x => x.SaleDate >= startUtc && x.SaleDate < endUtc)
        .SumAsync(x => x.Subtotal, ct);

    var monthStartLocal = new DateTime(date.Year, date.Month, 1);
    var monthEndLocal = monthStartLocal.AddMonths(1);

    var monthStartUtc = ConvertLocalToUtc(monthStartLocal, tz);
    var monthEndUtc = ConvertLocalToUtc(monthEndLocal, tz);

    var thisMonth = await query
        .Where(x => x.SaleDate >= monthStartUtc && x.SaleDate < monthEndUtc)
        .SumAsync(x => x.Subtotal, ct);

    return new SalesSummaryDto(
        total,
        thisMonth,
        today
    );
  }

  private static TimeZoneInfo GetTimeZone(string timeZone)
  {
    if (string.IsNullOrWhiteSpace(timeZone))
      throw new ArgumentException("Time zone is required.", nameof(timeZone));

    return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
  }

  private static DateTime ConvertLocalToUtc(DateTime localDateTime, TimeZoneInfo tz)
  {
    var localUnspecified = DateTime.SpecifyKind(
        localDateTime,
        DateTimeKind.Unspecified);

    return TimeZoneInfo.ConvertTimeToUtc(localUnspecified, tz);
  }

  private static DateTime ConvertUtcToLocal(DateTime utcDateTime, TimeZoneInfo tz)
  {
    var utc = DateTime.SpecifyKind(
        utcDateTime,
        DateTimeKind.Utc);

    return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
  }

  private static SaleDto ToDto(Sale s, TimeZoneInfo tz) =>
      new(
          s.SaleId,
          s.StoreId,
          s.UserId,
          ConvertUtcToLocal(s.SaleDate, tz),
          s.Subtotal,
          s.Tax,
          s.Total,
          s.PaymentMethod,
          s.Notes,
          ConvertUtcToLocal(s.CreatedAt, tz),
          s.UpdatedAt.HasValue
              ? ConvertUtcToLocal(s.UpdatedAt.Value, tz)
              : null
      );
}
