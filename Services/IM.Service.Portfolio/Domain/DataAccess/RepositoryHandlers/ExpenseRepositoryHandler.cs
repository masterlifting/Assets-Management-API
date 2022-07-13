using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class ExpenseRepositoryHandler : RepositoryHandler<Expense>
{
    private readonly DatabaseContext context;
    public ExpenseRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Expense>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Expense> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Value = New.Value;
            Old.Date = New.Date;
            Old.DealId = New.DealId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Expense> GetExist(IEnumerable<Expense> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Expenses.Where(x => ids.Contains(x.Id));
    }
}