using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.RabbitServices;

internal interface IRabbitRepositoryHandler
{
    Task<bool> GetRepositoryActionAsync<TEntity, TContext>(Repository<TEntity, TContext> repository, QueueActions action, object[] id, TEntity data) where TEntity : class where TContext : DbContext;
    Task<bool> GetRepositoryActionAsync<TEntity, TContext>(Repository<TEntity, TContext> repository, QueueActions action, IEnumerable<TEntity> data, IEqualityComparer<TEntity> comparer) where TEntity : class where TContext : DbContext;
}