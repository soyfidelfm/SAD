using Sad.Api.Contracts.Sales;

namespace Sad.Api.Services.Sales;

public interface ISalesService
{
  Task<SaleDto> CreateAsync(
      Guid userId,
      SaleCreateDto dto,
      string timeZone,
      CancellationToken ct);

  Task<SaleDto?> GetByIdAsync(
      Guid saleId,
      string timeZone,
      CancellationToken ct);

  Task<IReadOnlyList<SaleDto>> GetAsync(
      int? storeId,
      Guid? userId,
      DateTime? fromLocal,
      DateTime? toLocal,
      string timeZone,
      CancellationToken ct);

  Task<bool> UpdateAsync(
      Guid saleId,
      SaleUpdateDto dto,
      string timeZone,
      CancellationToken ct);

  Task<bool> DeleteAsync(
      Guid saleId,
      CancellationToken ct);

  Task<bool> DeleteByIdAsync(
      Guid saleId,
      CancellationToken ct);

  Task<SalesSummaryDto> GetSummaryAsync(
      Guid userId,
      DateOnly date,
      string timeZone,
      CancellationToken ct);

  Task<IReadOnlyList<SaleDto>> GetByStoreIdAsync(
      int storeId,
      string timeZone,
      CancellationToken ct);

  Task<IReadOnlyList<SaleDto>> GetByStoreAndDateAsync(
      int storeId,
      DateTime date,
      string timeZone,
      CancellationToken ct);

  Task<IReadOnlyList<SaleDto>> GetByStoreAndRangeAsync(
      int storeId,
      DateTime from,
      DateTime to,
      string timeZone,
      CancellationToken ct);

  Task<IReadOnlyList<SaleDto>> GetLatestAsync(
      int top,
      string timeZone,
      CancellationToken ct,
      Guid? userId = null);
}
