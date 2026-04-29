namespace SADWebApi.Contracts.Dashboard;

  public record LastTrasactionsDto
  (
    string TrasactionName,
    DateTime TrasactionDate,
    bool IsSuccessful
  );

