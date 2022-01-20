using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class BrokerRepository : RepositoryHandler<Entities.Broker, DatabaseContext>
{
    public BrokerRepository(DatabaseContext context) : base(context)
    {
    }
}