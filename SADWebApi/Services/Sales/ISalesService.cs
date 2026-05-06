using Sad.Api.Contracts.Sales;

namespace Sad.Api.Services.Sales;

public interface ISalesService
{
	Task<SaleDto> CreateAsync(Guid userId, SaleCreateDto dto, CancellationToken ct);
	Task<SaleDto?> GetByIdAsync(Guid saleId, CancellationToken ct);
	Task<IReadOnlyList<SaleDto>> GetAsync(int? storeId, Guid? userId,DateTime? fromUtc,DateTime? toUtc,CancellationToken ct);
	Task<bool> UpdateAsync(Guid saleId, SaleUpdateDto dto, CancellationToken ct);
	Task<bool> DeleteAsync(Guid saleId, CancellationToken ct);

  // ✅ existentes
  Task<SalesSummaryDto> GetSummaryAsync(Guid userId,DateOnly date,string timeZone,CancellationToken ct);
  Task<IReadOnlyList<SaleDto>> GetByStoreIdAsync(int storeId, CancellationToken ct);
	Task<IReadOnlyList<SaleDto>> GetByStoreAndDateAsync(int storeId, DateTime date, CancellationToken ct);
	Task<bool> DeleteByIdAsync(Guid saleId, CancellationToken ct);

	// ✅ NUEVO: rango real (from/to) — 1 sola llamada
	Task<IReadOnlyList<SaleDto>> GetByStoreAndRangeAsync(int storeId, DateTime from, DateTime to, CancellationToken ct);
  Task<IReadOnlyList<SaleDto>> GetLatestAsync(int top, CancellationToken ct, Guid? userId = null);
}
