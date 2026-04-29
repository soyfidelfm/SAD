using Sad.Api.Data.Entities;

namespace Sad.Api.Data.Entities.Sales;

public class Sale
{
  public Guid SaleId { get; set; }

  public int StoreId { get; set; }
  public Guid UserId { get; set; }
  public byte StatusId { get; set; }

  public DateTime SaleDate { get; set; }

  public decimal Subtotal { get; set; }
  public decimal Tax { get; set; }

  public decimal Total { get; private set; }

  public string? PaymentMethod { get; set; }
  public string? Notes { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

  public AuthUser User { get; set; } = null!;
  public CatalogStore Store { get; set; } = null!;
  public CatalogSaleStatus Status { get; set; } = null!;
}
