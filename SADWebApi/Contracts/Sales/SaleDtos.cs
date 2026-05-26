using System;

namespace Sad.Api.Contracts.Sales;

public record SaleDto(
	Guid SaleId,
	int StoreId,
	Guid UserId,
	DateTime SaleDate,
	decimal Subtotal,
	decimal Tax,
	decimal Total,
	string? PaymentMethod,
	string? Notes,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);

public record SaleCreateDto(
  int StoreId,
  DateTime? SaleDate,
  decimal Subtotal,
  decimal Tax,
  decimal Total,
  string? PaymentMethod,
  string? Notes
);

public record SaleUpdateDto(
	DateTime? SaleDate,
	decimal Subtotal,
	decimal Tax,
	string? PaymentMethod,
	string? Notes
);
