using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Services.Sales;
using SADWebApi.Contracts.Dashboard;
using SADWebApi.Contracts.Helpers;

namespace Sad.Api.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
	private readonly SadDbContext _db;
	private readonly ICreditCardApplicationsService _creditCardSvc;
	private readonly IMembershipSalesService _membershipSvc;

	public DashboardService(
		SadDbContext db,
		ICreditCardApplicationsService creditCardSvc,
		IMembershipSalesService membershipSvc)
	{
		_db = db;
		_creditCardSvc = creditCardSvc;
		_membershipSvc = membershipSvc;
	}

	public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken ct)
	{
		// ✅ 1) credit summary (asegúrate que internamente ya use PST→UTC)
		var creditCards = await _creditCardSvc.GetSummaryAsync(userId, ct);

		// ✅ 2) membership summary (ya corregido arriba)
		var memberships = await _membershipSvc.GetSummaryAsync(userId, ct);

		// ✅ 3) last login
		var lastLoginUtc = await _db.Users
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Select(x => x.LastLoginAtUtc)
			.SingleAsync(ct);

		// ✅ 4) sales summary for “today PST” (convertido a rango UTC)
		var (startUtc, endUtc) = DateTimeHelper.GetTodayUtcFromPst();

		var todaySalesTotal = await _db.Sales
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Where(x => x.SaleDate >= startUtc && x.SaleDate < endUtc)
			.SumAsync(x => (decimal?)x.Total, ct) ?? 0m;

		return new DashboardSummaryDto(
			creditCards,
			memberships,
			new LastLoginSummaryDto(lastLoginUtc.Value),
			new TodaySalesSummaryDto(todaySalesTotal)
		);
	}

	public async Task<IEnumerable<SalesByHourDto>> GetTodaySalesByHourAsync(Guid userId, CancellationToken ct)
	{
		// Obtener rango del día en PST convertido a UTC
		var (startUtc, endUtc) = DateTimeHelper.GetTodayUtcFromPst();

		// TimeZone PST
		var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

		var data = await _db.Sales
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Where(x => x.SaleDate >= startUtc && x.SaleDate < endUtc)
			.ToListAsync(ct);

		var result = data
			.GroupBy(x =>
			{
				// Convertir cada fecha a PST para agrupar correctamente por hora
				var pstDate = TimeZoneInfo.ConvertTimeFromUtc(x.SaleDate, pacificZone);
				return pstDate.Hour;
			})
			.Select(g => new SalesByHourDto
			{
				Hour = g.Key,
				HourLabel = FormatHour(g.Key),
				Total = g.Sum(x => x.Total)
			})
			.OrderBy(x => x.Hour)
			.ToList();

		return result;
	}
	private static string FormatHour(int hour)
	{
		var dt = DateTime.Today.AddHours(hour);
		return dt.ToString("h tt"); // 1 PM, 2 AM, etc.
	}
}
