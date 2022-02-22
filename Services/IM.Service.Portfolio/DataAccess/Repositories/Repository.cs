using IM.Service.Common.Net.RepositoryService;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class Repository<T> : Repository<T, DatabaseContext> where T : class
{
    public Repository(ILogger<Repository<T, DatabaseContext>> logger, DatabaseContext context, IRepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}