namespace SADWebApi.Contracts.Sales;

public sealed record ReceiptReadResultDto(
    decimal? Subtotal,
    decimal? Tax,
    decimal? Total,
    string? PaymentMethod,
    string? StoreNumber,
    DateTime? SaleDate,
    List<ReceiptItemDto> Items,
    string RawText
);
