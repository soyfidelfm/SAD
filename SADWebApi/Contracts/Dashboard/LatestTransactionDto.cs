namespace SADWebApi.Contracts.Dashboard;

  public record LatestTransactionDto
  (
    string TransactionName,
    DateTime TransactionDate,
    string Status
  );

