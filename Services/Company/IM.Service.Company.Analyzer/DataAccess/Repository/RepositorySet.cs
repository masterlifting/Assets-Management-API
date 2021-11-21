using IM.Service.Common.Net.RepositoryService;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class RepositorySet<T> : Repository<T, DatabaseContext> where T : class
{
    public RepositorySet(ILogger<Repository<T, DatabaseContext>> logger, DatabaseContext context, IRepositoryHandler<T> handler) :
        base(logger, context, handler)
    { }
}