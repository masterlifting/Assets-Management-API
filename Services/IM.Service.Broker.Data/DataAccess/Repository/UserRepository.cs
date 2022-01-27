using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class UserRepository : RepositoryHandler<User, DatabaseContext>
{
    public UserRepository(DatabaseContext context) : base(context)
    {
    }
}