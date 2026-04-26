namespace SADWebApi.Contracts.Sales;

public record CreateCreditCardApplicationDto(    
    int StoreId,
    int StoreNumber,
    bool Approved,
	int CreditCardProductId,
    byte StatusId
);
