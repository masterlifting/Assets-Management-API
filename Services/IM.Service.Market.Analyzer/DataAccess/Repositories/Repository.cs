using IM.Service.Common.Net.RepositoryService;
using Microsoft.Extensions.Logging;

namespace IM.Service.Market.Analyzer.DataAccess.Repositories;

public class Repository<T> : Repository<T, DatabaseContext> where T : class
{
    public Repository(ILogger<Repository<T>> logger, DatabaseContext context, IRepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}