using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.RepositoryService;

public abstract class RepositoryHandler<T, TContext> : IRepositoryHandler<T> where T : class where TContext : DbContext
{
    private readonly TContext context;
    protected RepositoryHandler(TContext context) => this.context = context;

    public virtual Task<T> RunCreateHandlerAsync(T entity) => Task.FromResult(entity);
    public virtual async Task<IEnumerable<T>> RunCreateRangeHandlerAsync(IEnumerable<T> entities, IEqualityComparer<T> comparer)
    {
        entities = entities.ToArray();
        var distinctEntities = entities.Distinct(comparer);
        var existEntities = await GetExist(distinctEntities).ToArrayAsync().ConfigureAwait(false);
        return entities.Except(existEntities, comparer);
    }

    public virtual async Task<T> RunUpdateHandlerAsync(object[] id, T entity) =>
        await context.Set<T>().FindAsync(id).ConfigureAwait(false) is null
            ? throw new SqlNullValueException(nameof(RunUpdateHandlerAsync))
            : entity;
    public virtual async Task<T> RunUpdateHandlerAsync(T entity) =>
        await context.Set<T>().FindAsync(entity).ConfigureAwait(false) is null
            ? throw new SqlNullValueException(nameof(RunUpdateHandlerAsync))
            : entity;
    public abstract Task<IEnumerable<T>> RunUpdateRangeHandlerAsync(IEnumerable<T> entities);

    public virtual Task RunPostProcessAsync(RepositoryActions action, T entity) => Task.CompletedTask;
    public virtual Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<T> entities) => Task.CompletedTask;

    public abstract IQueryable<T> GetExist(IEnumerable<T> entities);
}