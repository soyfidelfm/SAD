using Sad.Api.Contracts.Sales;
using SADWebApi.Contracts.Sales;
using System;

namespace Sad.Api.Services.Sales;

public interface ICreditCardApplicationsService
{
    Task<long> CreateAsync(Guid userId, CreateCreditCardApplicationDto dto, CancellationToken ct);
    Task<IReadOnlyList<CreditCardApplicationDto>> GetLatestAsync(int top, CancellationToken ct);
	Task<CreditCardApplicationsSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct);
}
