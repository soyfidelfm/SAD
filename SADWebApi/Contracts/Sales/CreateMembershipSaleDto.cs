namespace Sad.Api.Contracts.Sales;

public record CreateMembershipSaleDto(
    int StoreId,
    int MembershipProductId,
    byte StatusId
);
