namespace SADWebApi.Contracts.Dashboard;

public sealed record DashboardHistoryDto(
    DateOnly Date,
    decimal TotalSales,
    int CreditCards,
    int Memberships,
    decimal AverageSale,
    decimal GoalPercent
);
