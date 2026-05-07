namespace SADWebApi.Contracts.Dashboard;

public sealed record AnalyticsSummaryDto(
    decimal TotalSales,
    int CreditCards,
    int Memberships,
    decimal AverageSale,
    decimal GoalPercent,
    string? BestDay,
    string? BestHour,
    decimal HighestTransaction
);
