using System.ComponentModel.DataAnnotations;

namespace SADWebApi.Contracts.Stores;

public class StoreDto
{
  public int StoreId { get; set; }
  public int StoreNumber { get; set; }
  public string? StoreName { get; set; }
  public bool IsActive { get; set; }
}

public class UpsertStoreDto
{
  [Range(1, int.MaxValue)]
  public int StoreNumber { get; set; }

  [MaxLength(120)]
  public string? StoreName { get; set; }

  public bool IsActive { get; set; } = true;
}
