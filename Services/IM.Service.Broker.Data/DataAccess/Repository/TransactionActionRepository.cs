using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class TransactionActionRepository : RepositoryHandler<TransactionAction, DatabaseContext>
{
    public TransactionActionRepository(DatabaseContext context) : base(context)
    {
    }
}