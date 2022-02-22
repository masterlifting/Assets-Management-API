using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class DealRepository : RepositoryHandler<Deal, DatabaseContext>
{
    public DealRepository(DatabaseContext context) : base(context)
    {
    }
}