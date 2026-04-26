namespace Sad.Api.Contracts.Sales;

public record MembershipSalesSummaryDto(
	int Total,
	int Today
// si quieres luego: TotalRevenue, etc.
);
