namespace Sad.Api.Data.Entities;

public class CatalogMembershipProduct
{
    public int MembershipProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public bool IsActive { get; set; }
}
