public record SalesByHourByDateDto(
    DateOnly Date,
    int Hour,
    decimal TotalSales
);
