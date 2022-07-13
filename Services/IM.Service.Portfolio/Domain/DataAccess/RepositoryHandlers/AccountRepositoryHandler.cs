using IM.Service.Shared.SqlAccess;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class AccountRepositoryHandler : RepositoryHandler<Account>
{
    private readonly DatabaseContext context;
    public AccountRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Account>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Account> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        return existEntities
            .Join(entities, x => (x.UserId, x.ProviderId, x.Name), y => (y.UserId, y.ProviderId, y.Name), (x, _) => x)
            .ToArray();
    }
    public override IQueryable<Account> GetExist(IEnumerable<Account> entities)
    {
        entities = entities.ToArray();

        var names = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key);
        var userIds = entities
            .GroupBy(x => x.UserId)
            .Select(x => x.Key);
        var providerIds = entities
            .GroupBy(x => x.ProviderId)
            .Select(x => x.Key);

        return context.Accounts
            .Where(x =>
                userIds.Contains(x.UserId)
                && providerIds.Contains(x.ProviderId)
                && names.Contains(x.Name));
    }
}