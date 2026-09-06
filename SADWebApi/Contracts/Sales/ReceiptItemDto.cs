namespace SADWebApi.Contracts.Sales;

public sealed record ReceiptItemDto(
    string Sku,
    string Description
);
