using SADWebApi.Contracts.Sales;
using System.Globalization;
using System.Text.RegularExpressions;

public class ReceiptReaderService : IReceiptReaderService
{
  private readonly IReceiptOcrService _ocr;

  public ReceiptReaderService(IReceiptOcrService ocr)
  {
    _ocr = ocr;
  }

  public async Task<ReceiptReadResultDto> ReadReceiptAsync(Stream imageStream)
  {
    var rawText = await _ocr.ReadTextAsync(imageStream);

    var subtotal = ExtractSubtotal(rawText);
    var tax = ExtractSalesTax(rawText);
    var total = ExtractTotal(rawText);

    if (total == null && subtotal != null && tax != null)
    {
      total = Math.Round(subtotal.Value + tax.Value, 2);
    }

    return new ReceiptReadResultDto(
      subtotal,
      tax,
      total,
      ExtractPaymentMethod(rawText),
      ExtractStoreNumber(rawText),
      ExtractSaleDate(rawText),
      ExtractItems(rawText),
      rawText
    );
  }

  private static decimal? ExtractSubtotal(string text)
  {
    return ExtractAmountByLabel(
      text,
      "SUBTOTAL",
      "SUB TOTAL",
      "SubTotal"
    );
  }

  private static decimal? ExtractSalesTax(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null;

    var lines = NormalizeText(text)
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Select(x => Regex.Replace(x.Trim(), @"\s+", " "))
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .ToList();

    for (int i = 0; i < lines.Count; i++)
    {
      var line = lines[i];

      if (!Regex.IsMatch(line, @"SALES\s*TAX|SALE\s*TAX", RegexOptions.IgnoreCase))
        continue;

      var amountOnSameLine = ExtractLastMoneyAmount(line);

      if (amountOnSameLine != null)
        return amountOnSameLine;

      for (int j = i + 1; j <= i + 5 && j < lines.Count; j++)
      {
        var nextLine = lines[j];

        if (Regex.IsMatch(nextLine, @"BALANCE\s*TOTAL|TOTAL\s*DUE|^\s*TOTAL\b", RegexOptions.IgnoreCase))
          break;

        if (Regex.IsMatch(nextLine, @"\b\d{6,8}\b"))
          continue;

        if (Regex.IsMatch(nextLine, @"COMP\s*VALUE|SALE\s*DISCOUNT|SAVINGS", RegexOptions.IgnoreCase))
          continue;

        var amount = ExtractLastMoneyAmount(nextLine);

        if (amount != null && amount < 500)
          return amount;
      }
    }

    return null;
  }

  private static decimal? ExtractTotal(string text)
  {
    return ExtractAmountByLabel(
      text,
      "BALANCE TOTAL",
      "BALANCE DUE",
      "TOTAL DUE",
      "Balance Total",
      "Total Due"
    );
  }

  private static decimal? ExtractAmountByLabel(string text, params string[] labels)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null;

    var lines = NormalizeText(text)
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Select(x => Regex.Replace(x.Trim(), @"\s+", " "))
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .ToList();

    for (int i = 0; i < lines.Count; i++)
    {
      var line = lines[i];

      foreach (var label in labels)
      {
        var labelPattern = BuildLooseLabelPattern(label);

        if (!Regex.IsMatch(line, labelPattern, RegexOptions.IgnoreCase))
          continue;

        var amount = ExtractLastMoneyAmount(line);

        if (amount != null)
          return amount;

        for (int j = i + 1; j <= i + 3 && j < lines.Count; j++)
        {
          amount = ExtractLastMoneyAmount(lines[j]);

          if (amount != null)
            return amount;
        }
      }
    }

    return null;
  }

  private static string BuildLooseLabelPattern(string label)
  {
    var parts = label
      .Split(' ', StringSplitOptions.RemoveEmptyEntries)
      .Select(Regex.Escape);

    return string.Join(@"\s*", parts);
  }

  private static decimal? ExtractLastMoneyAmount(string line)
  {
    if (string.IsNullOrWhiteSpace(line))
      return null;

    var matches = Regex.Matches(
      line,
      @"\$?\s*(\d{1,6}[.,]\d{2})"
    );

    if (matches.Count == 0)
      return null;

    var value = matches[^1].Groups[1].Value
      .Replace(",", ".");

    return decimal.TryParse(
      value,
      NumberStyles.Number,
      CultureInfo.InvariantCulture,
      out var result
    )
      ? result
      : null;
  }

  private static decimal? ToDecimal(string value)
  {
    value = value
      .Replace("$", "")
      .Replace(",", ".")
      .Trim();

    return decimal.TryParse(
      value,
      NumberStyles.Number,
      CultureInfo.InvariantCulture,
      out var result
    )
      ? result
      : null;
  }

  private static string NormalizeText(string text)
  {
    return text
      .Replace("\r\n", "\n")
      .Replace("\r", "\n")
      .Replace("S U B T O T A L", "SUBTOTAL")
      .Replace("S A L E S  T A X", "SALES TAX")
      .Replace("T O T A L  D U E", "TOTAL DUE")
      .Replace("B A L A N C E  T O T A L", "BALANCE TOTAL");
  }

  private static string? ExtractPaymentMethod(string text)
  {
    string[] methods =
    [
      "APPLE PAY",
      "GOOGLE PAY",
      "MASTERCARD",
      "DISCOVER",
      "PAYPAL",
      "VISA",
      "AMEX",
      "DEBIT",
      "CASH"
    ];

    return methods.FirstOrDefault(method =>
      text.Contains(method, StringComparison.OrdinalIgnoreCase));
  }

  private static string? ExtractStoreNumber(string text)
  {
    var bestBuyMatch = Regex.Match(
      text,
      @"BEST\s*BUY\s*#\s*(\d+)",
      RegexOptions.IgnoreCase
    );

    if (bestBuyMatch.Success)
      return bestBuyMatch.Groups[1].Value;

    var storeMatch = Regex.Match(
      text,
      @"STORE\s*#?\s*(\d+)",
      RegexOptions.IgnoreCase
    );

    return storeMatch.Success
      ? storeMatch.Groups[1].Value
      : null;
  }

  private static DateTime? ExtractSaleDate(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return null;

    var match = Regex.Match(
        text,
        @"(\d{1,2}/\d{1,2}/\d{2,4})\s+(\d{1,2}:\d{2})"
    );

    if (!match.Success)
      return null;

    var rawDate = $"{match.Groups[1].Value} {match.Groups[2].Value}";

    string[] formats =
    [
        "MM/dd/yy HH:mm",
        "M/d/yy HH:mm",
        "MM/dd/yyyy HH:mm",
        "M/d/yyyy HH:mm"
    ];

    return DateTime.TryParseExact(
        rawDate,
        formats,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var dt
    )
        ? dt
        : null;
  }

  private static List<ReceiptItemDto> ExtractItems(string text)
  {
    var items = new List<ReceiptItemDto>();

    var lines = NormalizeText(text)
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Select(x => Regex.Replace(x.Trim(), @"\s+", " "))
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .ToList();

    foreach (var line in lines)
    {
      var posMatch = Regex.Match(
        line,
        @"^(\d{6,8})\s+(.+?)(?:\s+\d{1,6}[.,]\d{2})?$"
      );

      if (posMatch.Success)
      {
        var sku = posMatch.Groups[1].Value;
        var description = posMatch.Groups[2].Value.Trim();

        if (!string.IsNullOrWhiteSpace(description) &&
            !IsReceiptNoise(description) &&
            description != ">")
        {
          items.Add(new ReceiptItemDto(sku, description));
        }

        continue;
      }
    }

    if (items.Count > 0)
      return items;

    for (int i = 0; i < lines.Count - 1; i++)
    {
      var skuMatch = Regex.Match(
        lines[i],
        @"^(\d{6,8})\b"
      );

      if (!skuMatch.Success)
        continue;

      var sku = skuMatch.Groups[1].Value;
      var description = FindDescription(lines, i + 1);

      if (string.IsNullOrWhiteSpace(description))
        continue;

      items.Add(new ReceiptItemDto(
        sku,
        description
      ));
    }

    return items;
  }

  private static string? FindDescription(List<string> lines, int startIndex)
  {
    for (int i = startIndex; i < lines.Count; i++)
    {
      var line = lines[i];

      if (IsPrice(line))
        continue;

      if (IsReceiptNoise(line))
        continue;

      if (Regex.IsMatch(line, @"^\d{6,8}\b"))
        return null;

      return line;
    }

    return null;
  }

  private static bool IsPrice(string line)
  {
    return Regex.IsMatch(
      line,
      @"^\$?\d+([,.]\d{2})?$"
    );
  }

  private static bool IsReceiptNoise(string line)
  {
    string[] noise =
    [
      "SALES TAX",
      "SALE TAX",
      "TAX",
      "SUBTOTAL",
      "SUB TOTAL",
      "TOTAL",
      "TOTAL DUE",
      "BALANCE TOTAL",
      "BALANCE DUE",
      "WAS PRICE",
      "COMP VALUE",
      "SALE DISCOUNT",
      "DISCOUNT",
      "INSTALLMENT BILLING",
      "SERIAL#",
      "IMEI#",
      "REFERENCE NUMBER",
      "APPROVAL",
      "ENTER/SCAN ITEM",
      "POINT OF SALE",
      "CHECK FULFILLMENT",
      "PROTECTION PLANS",
      "SERVICES",
      "ACCESSORIES",
      "SUBSCRIPTIONS"
    ];

    return noise.Any(x =>
      line.Contains(x, StringComparison.OrdinalIgnoreCase));
  }
}
