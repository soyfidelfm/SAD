namespace Sad.Api.Data.Entities;

public class CatalogSaleStatus
{
  public byte StatusId { get; set; }

  public string StatusCode { get; set; } = string.Empty;
  public string StatusName { get; set; } = string.Empty;

  public ICollection<SalesCreditCardApplication> CreditCardApplications { get; set; } = new List<SalesCreditCardApplication>();
  public ICollection<SalesMembershipSale> MembershipSales { get; set; } = new List<SalesMembershipSale>();
}
