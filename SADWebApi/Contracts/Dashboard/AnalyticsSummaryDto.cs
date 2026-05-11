namespace SADWebApi.Contracts.Dashboard;

public sealed record AnalyticsSummaryDto(
    decimal TotalSales,
    int CreditCards,
    int Memberships,
    decimal AverageSale,
    decimal GoalPercent,
    decimal AppEfficiency,
    decimal MembershipEfficiency,
    string? BestDay,
    string? BestHour,
    decimal HighestTransaction
);
