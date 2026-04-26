using Sad.Api.Contracts.Sales;
using System;

namespace Sad.Api.Services.Sales;

public interface IMembershipSalesService
{
    Task<long> CreateAsync(Guid userId,CreateMembershipSaleDto dto, CancellationToken ct);
    Task<IReadOnlyList<MembershipSaleDto>> GetLatestAsync(int top, CancellationToken ct);
	Task<MembershipSalesSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct);
}
