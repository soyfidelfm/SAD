using Microsoft.AspNetCore.Mvc;
using SADWebApi.Contracts.Sales;

[ApiController]
[Route("api/receipt-reader")]
public class ReceiptReaderController : ControllerBase
{
  private readonly IReceiptReaderService _receiptReader;

  public ReceiptReaderController(IReceiptReaderService receiptReader)
  {
    _receiptReader = receiptReader;
  }

  [HttpPost("read")]
  [Consumes("multipart/form-data")]
  public async Task<ActionResult<ReceiptReadResultDto>> ReadReceipt(
      [FromForm] IFormFile image
  )
  {
    if (image == null || image.Length == 0)
      return BadRequest("Image is required.");

    await using var stream = image.OpenReadStream();

    var result = await _receiptReader.ReadReceiptAsync(stream);

    return Ok(result);
  }
}
