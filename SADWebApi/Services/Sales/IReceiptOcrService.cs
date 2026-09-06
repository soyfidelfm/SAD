using SADWebApi.Contracts.Sales;

public interface IReceiptOcrService
{
  Task<string> ReadTextAsync(Stream imageStream);  
}
