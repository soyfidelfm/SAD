namespace Sad.Api.Data.Entities;

public class CatalogSaleStatus
{
    public byte StatusId { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public bool IsFinal { get; set; }
    public bool IsActive { get; set; } = true; // si no existe en tu tabla, quítalo

    public ICollection<SalesCreditCardApplication> CreditCardApplications { get; set; } = new List<SalesCreditCardApplication>();
    public ICollection<SalesMembershipSale> MembershipSales { get; set; } = new List<SalesMembershipSale>();
}
