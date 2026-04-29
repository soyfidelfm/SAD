using Sad.Api.Contracts.Sales;
using System.Security.Cryptography.X509Certificates;

public record DashboardSummaryDto(
	CreditCardApplicationsSummaryDto CreditCards,
	MembershipSalesSummaryDto Memberships,
	LastLoginSummaryDto LastLogin,
	 TodaySalesSummaryDto TodaySales
);
