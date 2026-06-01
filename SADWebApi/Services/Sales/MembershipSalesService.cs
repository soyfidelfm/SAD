using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities;
using SADWebApi.Contracts.Helpers;
using SADWebApi.Services.Helpers;

namespace Sad.Api.Services.Sales;

public class MembershipSalesService : IMembershipSalesService
{
    private readonly SadDbContext _db;
    private readonly IHelpers _helpers;
  public MembershipSalesService(SadDbContext db,
    IHelpers helpers)
  {
    _db = db;
    _helpers = helpers;
  }

  public async Task<long> CreateAsync(Guid userId, CreateMembershipSaleDto dto, string timeZone, CancellationToken ct)
	{
		// opcional pero recomendado: validar que exista el usuario (mensaje claro)
		var userExists = await _db.Users.AnyAsync(u => u.UserId == userId, ct);
		if (!userExists)
			throw new InvalidOperationException($"User {userId} not found in auth.Users.");

		var entity = new SalesMembershipSale
		{
			UserId = userId,
			StoreId = dto.StoreId,
			MembershipProductId = dto.MembershipProductId,
			StatusId = dto.StatusId,
			SoldAtUtc = DateTime.UtcNow
		};

		_db.MembershipSales.Add(entity);
		await _db.SaveChangesAsync(ct);
		return entity.MembershipSaleId;
	}

    public async Task<IReadOnlyList<MembershipSaleDto>> GetLatestAsync(int top, string timeZone, CancellationToken ct, Guid? userId = null)
    {
    var tz = _helpers.GetTimeZone(timeZone);

    return await _db.MembershipSales
            .AsNoTracking()
            .Where(x => !userId.HasValue || x.UserId == userId.Value)
            .OrderByDescending(x => x.SoldAtUtc)
            .Take(top)
            .Select(x => new MembershipSaleDto(
                x.MembershipSaleId,
                x.UserId,
                x.StoreId,
                x.MembershipProductId,
                x.StatusId,
                x.Status.StatusName,
                _helpers.ConvertLocalToUtc(x.SoldAtUtc, tz)
            ))
            .ToListAsync(ct);
    }

  public async Task<MembershipSalesSummaryDto> GetSummaryAsync(
  Guid userId,
  DateOnly date,
  string timeZone,
  CancellationToken ct)
  {
    // Fallback seguro
    var tz = string.IsNullOrWhiteSpace(timeZone)
        ? TimeZoneInfo.Utc
        : TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    // Convertir día local → rango UTC
    var startLocal = date.ToDateTime(TimeOnly.MinValue);
    var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

    var total = await _db.MembershipSales
        .AsNoTracking()
        .Where(x => x.UserId == userId)
        .CountAsync(ct);

    var today = await _db.MembershipSales
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.SoldAtUtc >= startUtc &&
            x.SoldAtUtc < endUtc
        )
        .CountAsync(ct);

    // TOTAL DEL MES
    var monthStartLocal = new DateTime(date.Year, date.Month, 1);
    var monthEndLocal = monthStartLocal.AddMonths(1);

    var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(monthStartLocal, tz);
    var monthEndUtc = TimeZoneInfo.ConvertTimeToUtc(monthEndLocal, tz);

    var monthTotal = await _db.MembershipSales
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.SoldAtUtc >= monthStartUtc &&
            x.SoldAtUtc < monthEndUtc
        )
        .CountAsync(ct);

    return new MembershipSalesSummaryDto(total, monthTotal, today);
  }
}
