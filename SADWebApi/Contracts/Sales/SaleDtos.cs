using System;
using System.Text.Json.Serialization;

namespace Sad.Api.Contracts.Sales;

public record SaleDto(
  Guid SaleId,
  int StoreId,
  Guid UserId,
  DateTime SaleDate,

  [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  decimal Subtotal,

  [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  decimal Tax,

  [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  decimal Total,

  string? PaymentMethod,
  string? Notes,
  DateTime CreatedAt,
  DateTime? UpdatedAt
);

public record SaleCreateDto
{
  public int StoreId { get; init; }
  public DateTime? SaleDate { get; init; }
  public decimal Subtotal { get; init; }
  public decimal Tax { get; init; }
  public decimal Total { get; init; }
  public string? PaymentMethod { get; init; }
  public string? Notes { get; init; }
}

public record SaleUpdateDto(
  DateTime? SaleDate,

  [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  decimal Subtotal,

  [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  decimal Tax,

  string? PaymentMethod,
  string? Notes
);
