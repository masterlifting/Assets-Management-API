using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class AccountRepository : RepositoryHandler<Account, DatabaseContext>
{
    public AccountRepository(DatabaseContext context) : base(context)
    {
    }
}