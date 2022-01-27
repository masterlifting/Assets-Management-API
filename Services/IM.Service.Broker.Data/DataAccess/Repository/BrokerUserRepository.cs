using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class BrokerUserRepository : RepositoryHandler<BrokerUser, DatabaseContext>
{
    public BrokerUserRepository(DatabaseContext context) : base(context)
    {
    }
}