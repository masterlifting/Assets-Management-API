using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Market.Domain.DataAccess;

public class Repository<T> : Repository<T, DatabaseContext> where T : class
{
    public Repository(ILogger<Repository<T, DatabaseContext>> logger, DatabaseContext context, IRepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}