namespace Sad.Api.Contracts.Sales;

public record MembershipSalesSummaryDto(
	int Total,
  int ThisMonth,
  int Today
// si quieres luego: TotalRevenue, etc.
);
