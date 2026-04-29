namespace Sad.Api.Contracts.Sales;

public record MembershipSaleDto(
    long MembershipSaleId,
    Guid UserId,
    int StoreId,
    int MembershipProductId,
    byte StatusId,
    string StatusName,
    DateTime SoldAtUtc
);
