namespace Sad.Api.Contracts.Sales;

public record SalesSummaryDto(
  int Total,
  int ThisMonth,
  int Today
);
