using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Common.Net.RepositoryService;

public class RepositoryHandler<T, TContext> : IRepositoryHandler<T> where T : class where TContext : DbContext
{
    private readonly TContext context;
    protected RepositoryHandler(TContext context) => this.context = context;

    public virtual Task<T> GetCreateHandlerAsync(T entity) => Task.FromResult(entity);
    public virtual async Task<T> GetUpdateHandlerAsync(object[] id, T entity) =>
        await context.Set<T>().FindAsync(id).ConfigureAwait(false) is null
            ? throw new SqlNullValueException(nameof(GetCreateHandlerAsync))
            : entity;

    public virtual async Task<T> GetDeleteHandlerAsync(params object[] id)
    {
        var dbEntity = await context.Set<T>().FindAsync(id).ConfigureAwait(false);
        return dbEntity ?? throw new SqlNullValueException(nameof(GetDeleteHandlerAsync));
    }

    public virtual Task SetPostProcessAsync(T entity) => Task.CompletedTask;

    public virtual async Task<IEnumerable<T>> GetCreateRangeHandlerAsync(IEnumerable<T> entities, IEqualityComparer<T> comparer)
    {
        entities = entities.ToArray();
        var distinctEntities = entities.Distinct(comparer);
        var existEntities = await GetExist(distinctEntities).ToArrayAsync().ConfigureAwait(false);
        return entities.Except(existEntities, comparer);
    }
    public virtual Task<IEnumerable<T>> GetUpdateRangeHandlerAsync(IEnumerable<T> entities) => Task.FromResult(entities);
    public virtual Task<IEnumerable<T>> GetDeleteRangeHandlerAsync(IEnumerable<T> entities) => Task.FromResult(entities);
    public virtual Task SetPostProcessAsync(IReadOnlyCollection<T> entities) => Task.CompletedTask;

    public virtual IQueryable<T> GetExist(IEnumerable<T> entities) => context.Set<T>();
}