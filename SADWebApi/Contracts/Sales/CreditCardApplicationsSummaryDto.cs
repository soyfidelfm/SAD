namespace Sad.Api.Contracts.Sales;

public record CreditCardApplicationsSummaryDto(
	int Total,
	int Today,
	int Approved,
	int Declined,
	int Pending
);
