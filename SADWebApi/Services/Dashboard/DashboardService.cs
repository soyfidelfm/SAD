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

  public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
  Guid userId,
  DateOnly date,
  string timeZone,
  CancellationToken ct)
  {
    var creditCards = await _creditCardSvc.GetSummaryAsync(userId, date, timeZone, ct);
    var memberships = await _membershipSvc.GetSummaryAsync(userId, date, timeZone, ct);
    var sales = await _salesSvc.GetSummaryAsync(userId, date, timeZone, ct);

    var lastLoginUtc = await _db.Users
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .Select(x => x.LastLoginAtUtc)
      .SingleOrDefaultAsync(ct);

    return new DashboardSummaryDto(
      creditCards,
      memberships,
      new LastLoginSummaryDto(lastLoginUtc ?? DateTime.UtcNow),
      sales
    );
  }

  public async Task<IEnumerable<LatestTransactionDto>> GetLastTransactionsAsync(
    int top,
    DateOnly date,
    string timeZone,
    CancellationToken ct,
    Guid? userId = null)
  {
    var tz = string.IsNullOrWhiteSpace(timeZone)
        ? TimeZoneInfo.Utc
        : TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    var startLocal = date.ToDateTime(TimeOnly.MinValue);
    var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

    var creditCardTransactions = await _creditCardSvc.GetLatestAsync(top, ct, userId);
    var membershipTransactions = await _membershipSvc.GetLatestAsync(top, ct, userId);
    var salesTransactions = await _salesSvc.GetLatestAsync(top, ct, userId);

    var creditMapped = creditCardTransactions.Select(x => new
    {
      Type = "Credit Card",
      Amount = 0m,
      TransactionDateUtc = x.SubmittedAtUtc,
      Status = x.StatusName
    });

    var membershipMapped = membershipTransactions.Select(x => new
    {
      Type = "Membership",
      Amount = 0m,
      TransactionDateUtc = x.SoldAtUtc,
      Status = x.StatusName
    });

    var salesMapped = salesTransactions.Select(x => new
    {
      Type = "Sale",
      Amount = x.Total,
      TransactionDateUtc = x.SaleDate,
      Status = "Completed"
    });

    return creditMapped
        .Concat(membershipMapped)
        .Concat(salesMapped)
        .Where(x => x.TransactionDateUtc >= startUtc && x.TransactionDateUtc < endUtc)
        .OrderByDescending(x => x.TransactionDateUtc)
        .Take(top)
        .Select(x => new LatestTransactionDto(
            x.Type,
            x.Amount,
            TimeZoneInfo.ConvertTimeFromUtc(x.TransactionDateUtc, tz),
            x.Status
        ))
        .ToList();
  }



  public async Task<IEnumerable<SalesByHourDto>> GetTodaySalesByHourAsync(Guid userId, CancellationToken ct)
  {
    var (startUtcRaw, endUtcRaw) = DateTimeHelper.GetTodayUtcFromPst();

    var startUtc = DateTime.SpecifyKind(startUtcRaw, DateTimeKind.Utc);
    var endUtc = DateTime.SpecifyKind(endUtcRaw, DateTimeKind.Utc);

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

  /*ANALYTICS*/
  public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(
    Guid userId,
    DateOnly from,
    DateOnly to,
    string timeZone,
    CancellationToken ct)
  {
    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(from.ToDateTime(TimeOnly.MinValue), tz);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue), tz);
    /*SETTINGS BY USER*/
    var totalGoalAmount = await _db.UserDailySettings
    .AsNoTracking()
    .Where(x =>
        x.UserId == userId &&
        x.IsActive &&
        x.SettingDate >= from &&
        x.SettingDate <= to)
    .SumAsync(x => x.SalesGoalAmount, ct);
    /*SALES*/
    var sales = await _db.Sales
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.SaleDate >= startUtc &&
            x.SaleDate < endUtc)
        .ToListAsync(ct);
    /*CREDIT CARDS*/
    var creditCards = await _db.CreditCardApplications
        .AsNoTracking()
        .CountAsync(x =>
            x.UserId == userId &&
            x.SubmittedAtUtc >= startUtc &&
            x.SubmittedAtUtc < endUtc, ct);
    /*MEMBERSHIPS*/
    var memberships = await _db.MembershipSales
        .AsNoTracking()
        .CountAsync(x =>
            x.UserId == userId &&
            x.SoldAtUtc >= startUtc &&
            x.SoldAtUtc < endUtc, ct);

    var totalSales = sales.Sum(x => x.Total);
    var averageSale = sales.Any() ? sales.Average(x => x.Total) : 0;
    var highestTransaction = sales.Any() ? sales.Max(x => x.Total) : 0;
    var goalPercent = totalGoalAmount > 0 ? Math.Round((totalSales / totalGoalAmount) * 100, 2): 0;

    var salesWithLocalDate = sales.Select(x =>
    {
      var utc = DateTime.SpecifyKind(x.SaleDate, DateTimeKind.Utc);
      var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

      return new
      {
        Sale = x,
        LocalDate = DateOnly.FromDateTime(local),
        LocalHour = local.Hour
      };
    }).ToList();

    var bestHour = salesWithLocalDate
        .GroupBy(x => x.LocalHour)
        .Select(g => new
        {
          Hour = g.Key,
          TotalSales = g.Sum(x => x.Sale.Total)
        })
        .OrderByDescending(x => x.TotalSales)
        .FirstOrDefault();

    var bestDay = salesWithLocalDate
        .GroupBy(x => x.LocalDate)
        .Select(g => new
        {
          Date = g.Key,
          TotalSales = g.Sum(x => x.Sale.Total)
        })
        .OrderByDescending(x => x.TotalSales)
        .FirstOrDefault();

    var bestHourLabel = bestHour is not null
        ? $"{bestHour.Hour:00}:00"
        : null;

    var bestDayLabel = bestDay is not null
        ? bestDay.Date.ToString("yyyy-MM-dd")
        : null;

    return new AnalyticsSummaryDto(
        totalSales,
        creditCards,
        memberships,
        averageSale,
        goalPercent,        
        bestDayLabel,
        bestHourLabel,
        highestTransaction
    );
  }

  public async Task<IEnumerable<SalesByHourByDateDto>> GetSalesByHourByDateAsync(
    Guid userId,
    DateOnly from,
    DateOnly to,
    string timeZone,
    CancellationToken ct)
  {
    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(
        from.ToDateTime(TimeOnly.MinValue),
        tz
    );

    var endUtc = TimeZoneInfo.ConvertTimeToUtc(
        to.AddDays(1).ToDateTime(TimeOnly.MinValue),
        tz
    );

    var sales = await _db.Sales
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.SaleDate >= startUtc &&
            x.SaleDate < endUtc)
        .ToListAsync(ct);

    var result = sales
    .Select(x =>
    {
      var utc = DateTime.SpecifyKind(x.SaleDate, DateTimeKind.Utc);
      var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

      return new
      {
        Sale = x,
        LocalDate = DateOnly.FromDateTime(local),
        LocalHour = local.Hour
      };
    })
    .Where(x => x.LocalHour >= 10 && x.LocalHour <= 21)
    .GroupBy(x => new
    {
      x.LocalDate,
      x.LocalHour
    })
    .Select(g => new SalesByHourByDateDto(
        g.Key.LocalDate,
        g.Key.LocalHour,
        g.Sum(x => x.Sale.Total)
    ))
    .OrderByDescending(x => x.Date)
    .ThenBy(x => x.Hour)
    .ToList();

    return result;
  }

  public async Task<IEnumerable<DashboardHistoryDto>> GetHistoryAsync(
  Guid userId,
  DateOnly from,
  DateOnly to,
  string timeZone,
  CancellationToken ct)
  {
    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

    var startUtc = TimeZoneInfo.ConvertTimeToUtc(from.ToDateTime(TimeOnly.MinValue), tz);
    var endUtc = TimeZoneInfo.ConvertTimeToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue), tz);
    /*GOALS*/
    var goalsByDate = await _db.UserDailySettings
    .AsNoTracking()
    .Where(x =>
        x.UserId == userId &&
        x.IsActive)
    .ToDictionaryAsync(
        x => x.SettingDate,
        x => x.SalesGoalAmount,
        ct);
    /*MEMBERSHIPS*/
    var memberships = await _db.MembershipSales
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .ToListAsync(ct);

    var membershipsByDate = memberships
        .GroupBy(x =>
        {
          var utc = DateTime.SpecifyKind(x.SoldAtUtc, DateTimeKind.Utc);
          return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, tz));
        })
        .ToDictionary(g => g.Key, g => g.Count());
    /*CREDIT CARDS*/
    var creditCards = await _db.CreditCardApplications
    .AsNoTracking()
    .Where(x => x.UserId == userId)
    .ToListAsync(ct);

    var creditCardsByDate = creditCards
        .GroupBy(x =>
        {
          var utc = DateTime.SpecifyKind(x.SubmittedAtUtc, DateTimeKind.Utc);
          return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, tz));
        })
        .ToDictionary(g => g.Key, g => g.Count());
    /*SALES*/
    var sales = await _db.Sales
      .AsNoTracking()
      .Where(x =>
        x.UserId == userId &&
        x.SaleDate >= startUtc &&
        x.SaleDate < endUtc)
      .ToListAsync(ct);

    var result = sales
      .GroupBy(x =>
      {
        var utc = DateTime.SpecifyKind(x.SaleDate, DateTimeKind.Utc);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, tz));
      })
      .Select(g =>
      {
        membershipsByDate.TryGetValue(g.Key, out var membershipCount);
        creditCardsByDate.TryGetValue(g.Key, out var creditCardCount);
        goalsByDate.TryGetValue(g.Key, out var salesGoal);

        var totalSales = g.Sum(x => x.Total);

        var salesGoalPercent = salesGoal > 0
            ? Math.Round((totalSales / salesGoal) * 100, 2)
            : 0;

        return new DashboardHistoryDto(
            g.Key,
            totalSales,
            creditCardCount,
            membershipCount,            
            g.Average(x => x.Total),
            salesGoalPercent
        );
      })
      .OrderByDescending(x => x.Date)
      .ToList();

    return result;
  }

}
