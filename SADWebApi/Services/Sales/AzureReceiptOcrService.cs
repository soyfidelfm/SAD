using Azure;
using Azure.AI.Vision.ImageAnalysis;

public class AzureReceiptOcrService : IReceiptOcrService
{
  private readonly ImageAnalysisClient _client;

  public AzureReceiptOcrService(IConfiguration config)
  {
    var endpoint = config["AzureVision:Endpoint"];
    var key = config["AzureVision:Key"];

    if (string.IsNullOrWhiteSpace(endpoint))
      throw new InvalidOperationException("AzureVision:Endpoint is missing.");

    if (string.IsNullOrWhiteSpace(key))
      throw new InvalidOperationException("AzureVision:Key is missing.");

    _client = new ImageAnalysisClient(
        new Uri(endpoint),
        new AzureKeyCredential(key)
    );
  }

  public async Task<string> ReadTextAsync(Stream imageStream)
  {
    if (imageStream == null)
      throw new ArgumentNullException(nameof(imageStream));

    var imageData = BinaryData.FromStream(imageStream);

    var result = await _client.AnalyzeAsync(
        imageData,
        VisualFeatures.Read
    );

    var lines = result.Value.Read.Blocks
        .SelectMany(block => block.Lines)
        .Select(line => line.Text);

    return string.Join(Environment.NewLine, lines);
  }
}
