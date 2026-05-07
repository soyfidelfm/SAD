namespace Sad.Api.Contracts.Sales;

public record SalesSummaryDto(
  decimal Total,
  decimal ThisMonth,
  decimal Today
);
