using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class TransactionRepository : RepositoryHandler<Transaction, DatabaseContext>
{
    public TransactionRepository(DatabaseContext context) : base(context)
    {
    }
}