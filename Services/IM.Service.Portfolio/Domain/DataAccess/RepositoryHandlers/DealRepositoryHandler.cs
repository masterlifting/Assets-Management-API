using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class DealRepositoryHandler : RepositoryHandler<Deal>
{
    private readonly DatabaseContext context;
    public DealRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Deal>> RunUpdateRangeHandlerAsync(IEnumerable<Deal> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Cost = New.CurrencyId;
            Old.Value = New.Value;
            Old.Info = New.Info;
            Old.DerivativeId = New.DerivativeId;
            Old.ExchangeId = New.ExchangeId;
            Old.AccountBrokerId = New.AccountBrokerId;
            Old.AccountUserId = New.AccountUserId;
            Old.AccountName = New.AccountName;
            Old.OperationId = New.OperationId;
            Old.CurrencyId = New.CurrencyId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Deal> GetExist(IEnumerable<Deal> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Deals.Where(x => ids.Contains(x.Id));
    }
}