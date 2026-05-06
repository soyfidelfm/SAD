namespace SADWebApi.Contracts.Dashboard;

  public record LatestTransactionDto
  (
    string TransactionName,
    decimal? Amount,
    DateTime TransactionDate,
    string Status
  );

