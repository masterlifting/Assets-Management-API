using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class AccountRepository : RepositoryHandler<Account, DatabaseContext>
{
    public AccountRepository(DatabaseContext context) : base(context)
    {
    }
}