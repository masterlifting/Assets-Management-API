using IM.Service.Shared.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class UserRepositoryHandler : RepositoryHandler<User>
{
    private readonly DatabaseContext context;
    public UserRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<User>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<User> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<User> GetExist(IEnumerable<User> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Users.Where(x => ids.Contains(x.Id));
    }
}