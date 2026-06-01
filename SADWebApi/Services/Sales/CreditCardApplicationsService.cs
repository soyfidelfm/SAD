using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities;
using SADWebApi.Contracts.Sales;
using SADWebApi.Services.Helpers;

namespace Sad.Api.Services.Sales;

public class CreditCardApplicationsService : ICreditCardApplicationsService
{
  private readonly SadDbContext _db;
  private readonly IHelpers _helpers;

  public CreditCardApplicationsService(SadDbContext db, IHelpers helpers)
  {
    _db = db;
    _helpers = helpers;
  }

  public async Task<long> CreateAsync(Guid userId, CreateCreditCardApplicationDto dto, CancellationToken ct)
  {
    var entity = new SalesCreditCardApplication
    {
      UserId = userId,
      StoreId = dto.StoreId,
      CreditCardProductId = dto.CreditCardProductId,
      StatusId = dto.StatusId,
      SubmittedAtUtc = DateTime.UtcNow
    };

    _db.CreditCardApplications.Add(entity);
    await _db.SaveChangesAsync(ct);

    return entity.CreditCardApplicationId;
  }

  public async Task<IReadOnlyList<CreditCardApplicationDto>> GetLatestAsync(
      int top,
      string timeZone,
      CancellationToken ct,
      Guid? userId = null)
  {
    var tz = _helpers.GetTimeZone(timeZone);

    return await _db.CreditCardApplications
        .AsNoTracking()
        .Where(x => !userId.HasValue || x.UserId == userId.Value)
        .OrderByDescending(x => x.SubmittedAtUtc)
        .Take(top)
        .Select(x => new CreditCardApplicationDto(
            x.CreditCardApplicationId,
            x.UserId,
            x.StoreId,
            x.CreditCardProductId,
            x.StatusId,
            x.Status.StatusName,
            _helpers.ConvertLocalToUtc(x.SubmittedAtUtc, tz),
            x.Store.StoreName ?? "",
            x.Store.StoreNumber
        ))
        .ToListAsync(ct);
  }

  public async Task<CreditCardApplicationsSummaryDto> GetSummaryAsync(
  Guid userId,
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

    var query = _db.CreditCardApplications
        .AsNoTracking()
        .Where(x => x.UserId == userId);

    // TOTAL GENERAL
    var total = await query.CountAsync(ct);

    // TOTAL DEL DÍA
    var todayCount = await query
        .Where(x => x.SubmittedAtUtc >= startUtc && x.SubmittedAtUtc < endUtc)
        .CountAsync(ct);

    // TOTAL DEL MES
    var monthStartLocal = new DateTime(date.Year, date.Month, 1);
    var monthEndLocal = monthStartLocal.AddMonths(1);

    var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(monthStartLocal, tz);
    var monthEndUtc = TimeZoneInfo.ConvertTimeToUtc(monthEndLocal, tz);

    var monthCount = await query
        .Where(x => x.SubmittedAtUtc >= monthStartUtc && x.SubmittedAtUtc < monthEndUtc)
        .CountAsync(ct);

    var approved = await query
        .Where(x => x.Status.StatusCode == "APPROVED")
        .CountAsync(ct);

    var declined = await query
        .Where(x => x.Status.StatusCode == "DECLINED")
        .CountAsync(ct);

    var pending = await query
        .Where(x => x.Status.StatusCode == "PENDING")
        .CountAsync(ct);

    return new CreditCardApplicationsSummaryDto(
        total,
        monthCount,
        todayCount,
        approved,
        declined,
        pending
    );
  }
}
