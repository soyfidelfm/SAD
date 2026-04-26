namespace Sad.Api.Data.Entities;

public class CatalogCreditCardProduct
{
    public int CreditCardProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public bool IsActive { get; set; }
}
