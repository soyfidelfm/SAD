using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Sales;
using Sad.Api.Data;
using Sad.Api.Data.Entities;
using SADWebApi.Contracts.Sales;

namespace Sad.Api.Services.Sales;

public class CreditCardApplicationsService : ICreditCardApplicationsService
{
  private readonly SadDbContext _db;

  public CreditCardApplicationsService(SadDbContext db)
  {
    _db = db;
  }

  public async Task<long> CreateAsync(Guid userId, CreateCreditCardApplicationDto dto, CancellationToken ct)
  {
    var entity = new SalesCreditCardApplication
    {
      UserId = userId,
      StoreId = dto.StoreId,
      CreditCardProductId = dto.CreditCardProductId,
      StatusId = dto.StatusId,
      SubmittedAtUtc = DateTime.UtcNow
    };

    _db.CreditCardApplications.Add(entity);
    await _db.SaveChangesAsync(ct);

    return entity.CreditCardApplicationId;
  }

  public async Task<IReadOnlyList<CreditCardApplicationDto>> GetLatestAsync(
      int top,
      CancellationToken ct,
      Guid? userId = null)
  {
    return await _db.CreditCardApplications
        .AsNoTracking()
        .Where(x => !userId.HasValue || x.UserId == userId.Value)
        .OrderByDescending(x => x.SubmittedAtUtc)
        .Take(top)
        .Select(x => new CreditCardApplicationDto(
            x.CreditCardApplicationId,
            x.UserId,
            x.StoreId,
            x.CreditCardProductId,
            x.StatusId,
            x.Status.StatusName,
            x.SubmittedAtUtc,
            x.Store.StoreName ?? "",
            x.Store.StoreNumber
        ))
        .ToListAsync(ct);
  }

  public async Task<CreditCardApplicationsSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct)
  {
    var today = DateTime.UtcNow.Date;

    var query = _db.CreditCardApplications
        .AsNoTracking()
        .Where(x => x.UserId == userId);

    var total = await query.CountAsync(ct);

    var todayCount = await query
        .Where(x => x.SubmittedAtUtc.Date == today)
        .CountAsync(ct);

    var approved = await query
        .Where(x => x.Status.StatusCode == "APPROVED")
        .CountAsync(ct);

    var declined = await query
        .Where(x => x.Status.StatusCode == "DECLINED")
        .CountAsync(ct);

    var pending = await query
        .Where(x => x.Status.StatusCode == "PENDING")
        .CountAsync(ct);

    return new CreditCardApplicationsSummaryDto(
        total,
        todayCount,
        approved,
        declined,
        pending
    );
  }
}
