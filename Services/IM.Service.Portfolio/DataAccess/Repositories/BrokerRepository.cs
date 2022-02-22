using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class BrokerRepository : RepositoryHandler<Broker, DatabaseContext>
{
    public BrokerRepository(DatabaseContext context) : base(context)
    {
    }
}