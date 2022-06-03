using IM.Service.Shared.RepositoryService;
using Microsoft.Extensions.Logging;

namespace IM.Service.Recommendations.Domain.DataAccess;

public class Repository<T> : Repository<T, DatabaseContext> where T : class
{
    public Repository(ILogger<T> logger, DatabaseContext context, RepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}