namespace Sad.Api.Data.Entities;

public class SalesMembershipSale
{
  public long MembershipSaleId { get; set; }

  public Guid UserId { get; set; }
  public int StoreId { get; set; }
  public int MembershipProductId { get; set; }
  public byte StatusId { get; set; }

  public DateTime SoldAtUtc { get; set; }

  public AuthUser User { get; set; } = null!;
  public CatalogStore Store { get; set; } = null!;
  public CatalogMembershipProduct MembershipProduct { get; set; } = null!;
  public CatalogSaleStatus Status { get; set; } = null!;
}
