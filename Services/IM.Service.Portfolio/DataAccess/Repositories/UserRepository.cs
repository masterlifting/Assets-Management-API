using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class UserRepository : RepositoryHandler<User, DatabaseContext>
{
    public UserRepository(DatabaseContext context) : base(context)
    {
    }
}