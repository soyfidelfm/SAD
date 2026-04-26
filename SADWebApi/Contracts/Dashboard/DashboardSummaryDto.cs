using Sad.Api.Contracts.Sales;

public record DashboardSummaryDto(
	CreditCardApplicationsSummaryDto CreditCards,
	MembershipSalesSummaryDto Memberships,
	LastLoginSummaryDto LastLogin,
	 TodaySalesSummaryDto TodaySales
);