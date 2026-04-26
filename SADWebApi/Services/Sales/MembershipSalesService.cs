using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities;
using SADWebApi.Contracts.Helpers;

namespace Sad.Api.Services.Sales;

public class MembershipSalesService : IMembershipSalesService
{
    private readonly SadDbContext _db;
    public MembershipSalesService(SadDbContext db) => _db = db;

	public async Task<long> CreateAsync(Guid userId, CreateMembershipSaleDto dto, CancellationToken ct)
	{
		// opcional pero recomendado: validar que exista el usuario (mensaje claro)
		var userExists = await _db.Users.AnyAsync(u => u.UserId == userId, ct);
		if (!userExists)
			throw new InvalidOperationException($"User {userId} not found in auth.Users.");

		var entity = new SalesMembershipSale
		{
			UserId = userId,
			StoreId = dto.StoreId,
			MembershipProductId = dto.MembershipProductId,
			StatusId = dto.StatusId,
			SoldAtUtc = DateTime.UtcNow
		};

		_db.MembershipSales.Add(entity);
		await _db.SaveChangesAsync(ct);
		return entity.MembershipSaleId;
	}

    public async Task<IReadOnlyList<MembershipSaleDto>> GetLatestAsync(int top, CancellationToken ct)
    {
        return await _db.MembershipSales
            .AsNoTracking()
            .OrderByDescending(x => x.SoldAtUtc)
            .Take(top)
            .Select(x => new MembershipSaleDto(
                x.MembershipSaleId,
                x.UserId,
                x.StoreId,
                x.MembershipProductId,
                x.StatusId,
                x.SoldAtUtc
            ))
            .ToListAsync(ct);
    }

	public async Task<MembershipSalesSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct)
	{
		// “Hoy” definido en PST, convertido a rango UTC
		var (startUtc, endUtc) = DateTimeHelper.GetTodayUtcFromPst();

		var r = await _db.MembershipSales
			.AsNoTracking()
			.Where(x =>
				x.UserId == userId &&
				x.SoldAtUtc >= startUtc &&
				x.SoldAtUtc < endUtc
			)
			.GroupBy(_ => 1)
			.Select(g => new
			{
				Total = g.Count(),
				Today = g.Count() // ya es “hoy” por el WHERE
			})
			.FirstOrDefaultAsync(ct);

		return r is null
			? new MembershipSalesSummaryDto(0, 0)
			: new MembershipSalesSummaryDto(r.Total, r.Today);
	}
}
