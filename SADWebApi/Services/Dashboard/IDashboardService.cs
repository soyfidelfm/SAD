using SADWebApi.Contracts.Dashboard;

namespace Sad.Api.Services.Dashboard;

public interface IDashboardService
{
	Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, DateOnly date, string timeZone, CancellationToken ct);
	Task<IEnumerable<SalesByHourDto>> GetTodaySalesByHourAsync(Guid userId,	CancellationToken ct);
  Task<IEnumerable<LatestTransactionDto>> GetLastTransactionsAsync(int top, DateOnly date, string timeZone, CancellationToken ct, Guid? userId = null);
  Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(
    Guid userId,
    DateOnly from,
    DateOnly to,
    string timeZone,
    CancellationToken ct);

  Task<IEnumerable<DashboardHistoryDto>> GetHistoryAsync(
      Guid userId,
      DateOnly from,
      DateOnly to,
      string timeZone,
      CancellationToken ct);
}
