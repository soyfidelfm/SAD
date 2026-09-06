using Sad.Api.Contracts.Sales;
using System;

namespace Sad.Api.Services.Sales;

public interface IMembershipSalesService
{
    Task<long> CreateAsync(Guid userId,CreateMembershipSaleDto dto, string timeZone, CancellationToken ct);
    Task<IReadOnlyList<MembershipSaleDto>> GetLatestAsync(int top, string timeZone, CancellationToken ct, Guid? userId = null);
	Task<MembershipSalesSummaryDto> GetSummaryAsync(Guid userId, DateOnly date, string timeZone, CancellationToken ct);
  }
