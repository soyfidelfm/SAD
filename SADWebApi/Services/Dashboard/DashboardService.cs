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
  private readonly ISalesService _salesSvc;

  public DashboardService(
    SadDbContext db,
    ICreditCardApplicationsService creditCardSvc,
    IMembershipSalesService membershipSvc, ISalesService salesSvc)
  {
    _db = db;
    _creditCardSvc = creditCardSvc;
    _membershipSvc = membershipSvc;
    _salesSvc = salesSvc;
  }

  public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken ct)
  {
    var creditCards = await _creditCardSvc.GetSummaryAsync(userId, ct);
    var memberships = await _membershipSvc.GetSummaryAsync(userId, ct);

    var lastLoginUtc = await _db.Users
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .Select(x => x.LastLoginAtUtc)
      .SingleOrDefaultAsync(ct);

    var (startUtcRaw, endUtcRaw) = DateTimeHelper.GetTodayUtcFromPst();

    var startUtc = DateTime.SpecifyKind(startUtcRaw, DateTimeKind.Unspecified);
    var endUtc = DateTime.SpecifyKind(endUtcRaw, DateTimeKind.Unspecified);

    var todaySalesTotal = await _db.Sales
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .Where(x => x.SaleDate >= startUtc && x.SaleDate < endUtc)
      .SumAsync(x => (decimal?)x.Total, ct) ?? 0m;

    return new DashboardSummaryDto(
      creditCards,
      memberships,
      new LastLoginSummaryDto(lastLoginUtc ?? DateTime.UtcNow),
      new TodaySalesSummaryDto(todaySalesTotal)
    );
  }

  public async Task<IEnumerable<LatestTransactionDto>> GetLastTransactionsAsync(int top, CancellationToken ct, Guid? userId = null)
  {
    var creditCardTransactions = await _creditCardSvc.GetLatestAsync(top, ct, userId);
    var membershipTransactions = await _membershipSvc.GetLatestAsync(top, ct, userId);
    var salesTransactions = await _salesSvc.GetLatestAsync(top, ct, userId);

    // 🔹 MAPEAR cada fuente
    var creditMapped = creditCardTransactions.Select(x => new LatestTransactionDto(
        "Credit Card",
        x.SubmittedAtUtc,          // <-- ajusta al campo real
        x.StatusName         // <-- ajusta al campo real
    ));

    var membershipMapped = membershipTransactions.Select(x => new LatestTransactionDto(
        "Membership",
        x.SoldAtUtc,
        x.StatusName           // o lo que aplique
    ));

    var salesMapped = salesTransactions.Select(x => new LatestTransactionDto(
        "Sale",
        x.SaleDate,
        "Completed"           // o lo que aplique
    ));

    // 🔹 UNIR + ORDENAR + LIMITAR
    return creditMapped
        .Concat(membershipMapped)
        .Concat(salesMapped)
        .OrderByDescending(x => x.TransactionDate)
        .Take(top)
        .ToList();
  }



  public async Task<IEnumerable<SalesByHourDto>> GetTodaySalesByHourAsync(Guid userId, CancellationToken ct)
  {
    var (startUtcRaw, endUtcRaw) = DateTimeHelper.GetTodayUtcFromPst();

    var startUtc = DateTime.SpecifyKind(startUtcRaw, DateTimeKind.Unspecified);
    var endUtc = DateTime.SpecifyKind(endUtcRaw, DateTimeKind.Unspecified);

    var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

    var data = await _db.Sales
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .Where(x => x.SaleDate >= startUtc && x.SaleDate < endUtc)
      .ToListAsync(ct);

    var result = data
      .GroupBy(x =>
      {
        var saleUtc = DateTime.SpecifyKind(x.SaleDate, DateTimeKind.Utc);
        var pstDate = TimeZoneInfo.ConvertTimeFromUtc(saleUtc, pacificZone);
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
    return dt.ToString("h tt");
  }

}
