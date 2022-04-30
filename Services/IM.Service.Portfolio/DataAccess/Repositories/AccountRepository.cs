using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class AccountRepository : RepositoryHandler<Account, DatabaseContext>
{
    private readonly DatabaseContext context;
    public AccountRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Account>> RunUpdateRangeHandlerAsync(IEnumerable<Account> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        return existEntities
            .Join(entities, x => (x.UserId, x.BrokerId, x.Name), y => (y.UserId, y.BrokerId, y.Name), (x, _) => x)
            .ToArray();
    }
    public override IQueryable<Account> GetExist(IEnumerable<Account> entities)
    {
        entities = entities.ToArray();

        var userIds = entities
            .GroupBy(x => x.UserId)
            .Select(x => x.Key);
        var brockerIds = entities
            .GroupBy(x => x.BrokerId)
            .Select(x => x.Key);
        var names = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key);

        return context.Accounts
            .Where(x =>
                userIds.Contains(x.UserId)
                && brockerIds.Contains(x.BrokerId)
                && names.Contains(x.Name));
    }
}