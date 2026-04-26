namespace Sad.Api.Contracts.Sales;

public record CreditCardApplicationDto(
    long CreditCardApplicationId,
    Guid UserId,
    int StoreId,
    int CreditCardProductId,
    byte StatusId,
    DateTime SubmittedAtUtc,
    string StoreName,
    int StoreNumber
);
