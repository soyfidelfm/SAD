namespace Sad.Api.Data.Entities;

public class CatalogStore
{
    public int StoreId { get; set; }
    public int StoreNumber { get; set; }
    public string? StoreName { get; set; }
    public bool IsActive { get; set; }
}
