using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities;
using SADWebApi.Contracts.Helpers;
using SADWebApi.Contracts.Sales;

namespace Sad.Api.Services.Sales;

public class CreditCardApplicationsService : ICreditCardApplicationsService
{
    private readonly SadDbContext _db;
    public CreditCardApplicationsService(SadDbContext db) => _db = db;

	public async Task<long> CreateAsync(
	Guid userId,
	CreateCreditCardApplicationDto dto,
	CancellationToken ct)
	{
		var entity = new SalesCreditCardApplication
		{
			UserId = userId, // 👈 viene del JWT
			StoreId = dto.StoreId,
			CreditCardProductId = dto.CreditCardProductId,
			StatusId = dto.StatusId,
			SubmittedAtUtc = DateTime.UtcNow
		};

		_db.CreditCardApplications.Add(entity);
		await _db.SaveChangesAsync(ct);

		return entity.CreditCardApplicationId;
	}

	public async Task<IReadOnlyList<CreditCardApplicationDto>> GetLatestAsync(int top, CancellationToken ct)
    {
        return await _db.CreditCardApplications
            .AsNoTracking()
			.Include(x => x.Store)
			.OrderByDescending(x => x.SubmittedAtUtc)
            .Take(top)
            .Select(x => new CreditCardApplicationDto(
                x.CreditCardApplicationId,
                x.UserId,
                x.StoreId,
                x.CreditCardProductId,
                x.StatusId,
                x.SubmittedAtUtc,
				x.Store.StoreName?? "",
				x.Store.StoreNumber
            ))
            .ToListAsync(ct);
    }
	public async Task<CreditCardApplicationsSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct)
	{
		// IDs reales (ajusta si cambian)
		const int PendingId = 2;
		const int ApprovedId = 1;
		const int DeclinedId = 3;

		// ✅ "Hoy" de negocio en PST, convertido a rango UTC
		var (startUtc, endUtc) = DateTimeHelper.GetTodayUtcFromPst_DstSafe();

		var r = await _db.CreditCardApplications
			.AsNoTracking()
			.Where(x =>
				x.UserId == userId &&
				x.SubmittedAtUtc >= startUtc &&
				x.SubmittedAtUtc < endUtc
			)
			.GroupBy(_ => 1)
			.Select(g => new
			{
				Total = g.Count(),
				Today = g.Count(), // ✅ ya es "hoy PST" porque filtramos por rango
				Approved = g.Count(x => x.StatusId == ApprovedId),
				Declined = g.Count(x => x.StatusId == DeclinedId),
				Pending = g.Count(x => x.StatusId == PendingId)
			})
			.FirstOrDefaultAsync(ct);

		return r is null
			? new CreditCardApplicationsSummaryDto(0, 0, 0, 0, 0)
			: new CreditCardApplicationsSummaryDto(
				r.Total,
				r.Today,
				r.Approved,
				r.Declined,
				r.Pending
			);
	}

}
