namespace Sad.Api.Data.Entities;

public class SalesCreditCardApplication
{
    public long CreditCardApplicationId { get; set; }
    public Guid UserId { get; set; }
    public int StoreId { get; set; }
    public int CreditCardProductId { get; set; }
    public byte StatusId { get; set; }
    public DateTime SubmittedAtUtc { get; set; }

    public AuthUser User { get; set; } = null!;
    public CatalogStore Store { get; set; } = null!;
    public CatalogCreditCardProduct CreditCardProduct { get; set; } = null!;
    public CatalogSaleStatus Status { get; set; } = null!;
}
