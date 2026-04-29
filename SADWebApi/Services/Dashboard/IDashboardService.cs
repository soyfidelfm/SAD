using SADWebApi.Contracts.Dashboard;

namespace Sad.Api.Services.Dashboard;

public interface IDashboardService
{
	Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken ct);
	Task<IEnumerable<SalesByHourDto>> GetTodaySalesByHourAsync(Guid userId,	CancellationToken ct);
  Task<IEnumerable<LatestTransactionDto>> GetLastTransactionsAsync(int top, CancellationToken ct, Guid? userId);
}
