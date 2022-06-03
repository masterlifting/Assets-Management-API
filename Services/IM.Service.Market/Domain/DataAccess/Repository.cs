using IM.Service.Shared.RepositoryService;

namespace IM.Service.Market.Domain.DataAccess;

public class Repository<T> : Repository<T, DatabaseContext> where T : class
{
    public Repository(ILogger<T> logger, DatabaseContext context, RepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}