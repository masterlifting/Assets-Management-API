using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Shared.Enums;

namespace IM.Service.Shared.SqlAccess;

public abstract class RepositoryHandler<T> where T : class
{
    public virtual Task<T> RunCreateHandlerAsync(T entity) => Task.FromResult(entity);
    public virtual async Task<IEnumerable<T>> RunCreateRangeHandlerAsync(IReadOnlyCollection<T> entities, IEqualityComparer<T> comparer)
    {
        var distinctEntities = entities.Distinct(comparer);
        var existEntities = await GetExist(distinctEntities).ToArrayAsync().ConfigureAwait(false);
        return entities.Except(existEntities, comparer);
    }

    public virtual Task<T> RunUpdateHandlerAsync(object[] id, T entity) => Task.FromResult(entity);
    public abstract Task<IEnumerable<T>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<T> entities);

    public virtual Task RunPostProcessAsync(RepositoryActions action, T entity) => Task.CompletedTask;
    public virtual Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<T> entities) => Task.CompletedTask;

    public abstract IQueryable<T> GetExist(IEnumerable<T> entities);
}