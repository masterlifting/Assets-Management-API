using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.RepositoryService;

public abstract class RepositoryHandler<T> where T : class
{
    public virtual Task<T> RunCreateHandlerAsync(T entity) => Task.FromResult(entity);
    public virtual async Task<IEnumerable<T>> RunCreateRangeHandlerAsync(IEnumerable<T> entities, IEqualityComparer<T> comparer)
    {
        entities = entities.ToArray();
        var distinctEntities = entities.Distinct(comparer);
        var existEntities = await GetExist(distinctEntities).ToArrayAsync().ConfigureAwait(false);
        return entities.Except(existEntities, comparer);
    }

    public virtual Task<T> RunUpdateHandlerAsync(object[] id, T entity) => Task.FromResult(entity);
    public abstract Task<IEnumerable<T>> RunUpdateRangeHandlerAsync(IEnumerable<T> entities);

    public virtual Task RunPostProcessAsync(RepositoryActions action, T entity) => Task.CompletedTask;
    public virtual Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<T> entities) => Task.CompletedTask;

    public abstract IQueryable<T> GetExist(IEnumerable<T> entities);
}