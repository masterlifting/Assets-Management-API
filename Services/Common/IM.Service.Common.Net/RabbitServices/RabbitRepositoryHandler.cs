using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Common.Net.RabbitServices;

public abstract class RabbitRepositoryHandler : IRabbitRepositoryHandler
{
    public virtual async Task<bool> GetRepositoryActionAsync<TEntity, TContext>(
        Repository<TEntity, TContext> repository,
        QueueActions action,
        object[] id,
        TEntity data) where TEntity : class where TContext : DbContext
    {
        StringBuilder builder = new(nameof(RabbitRepositoryHandler), 200);
        builder.Append('.');
        builder.Append(action);
        builder.Append(".Id: ");
        builder.Append('\'');
        builder.Append(string.Join(",", id));
        builder.Append('\'');

        var info = builder.ToString();

        return action switch
        {
            QueueActions.Create => (await repository.CreateAsync(data, info)).error is null,
            QueueActions.CreateUpdate => (await repository.CreateUpdateAsync(id, data, info)).error is null,
            QueueActions.Update => (await repository.UpdateAsync(id, data, info)).error is null,
            QueueActions.Delete => (await repository.DeleteAsync(id, info )).error is null,
            _ => true
        };
    }

    public virtual async Task<bool> GetRepositoryActionAsync<TEntity, TContext>(
        Repository<TEntity, TContext> repository,
        QueueActions action,
        IEnumerable<TEntity> data,
        IEqualityComparer<TEntity> comparer) where TEntity : class where TContext : DbContext
    {
        StringBuilder builder = new(nameof(RabbitRepositoryHandler), 200);
        builder.Append('.');
        builder.Append(action);

        var info = builder.ToString();

        return action switch
        {
            QueueActions.Create => (await repository.CreateAsync(data, comparer, info)).error is null,
            QueueActions.CreateUpdate => (await repository.CreateUpdateAsync(data, comparer, info)).error is null,
            QueueActions.CreateUpdateDelete => (await repository.CreateUpdateDeleteAsync(data, comparer, info)).error is null,
            QueueActions.Update => (await repository.UpdateAsync(data, info)).error is null,
            QueueActions.Delete => (await repository.DeleteAsync(data, comparer, info)).error is null,
            _ => true
        };
    }
}