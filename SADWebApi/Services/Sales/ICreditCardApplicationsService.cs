using Sad.Api.Contracts.Sales;
using SADWebApi.Contracts.Sales;

namespace Sad.Api.Services.Sales;

public interface ICreditCardApplicationsService
{
  Task<long> CreateAsync(Guid userId, CreateCreditCardApplicationDto dto, CancellationToken ct);

  Task<IReadOnlyList<CreditCardApplicationDto>> GetLatestAsync(
      int top,
      string timeZone,
      CancellationToken ct,
      Guid? userId = null);

  Task<CreditCardApplicationsSummaryDto> GetSummaryAsync(
      Guid userId,
      DateOnly date,
      string timeZone,
      CancellationToken ct);
}
