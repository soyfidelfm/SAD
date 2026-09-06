using SADWebApi.Contracts.Sales;

public interface IReceiptReaderService
{
  Task<ReceiptReadResultDto> ReadReceiptAsync(Stream imageStream);
}
