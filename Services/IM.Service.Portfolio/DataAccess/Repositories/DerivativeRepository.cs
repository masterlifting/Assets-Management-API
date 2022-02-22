using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class DerivativeRepository : RepositoryHandler<Derivative, DatabaseContext>
{
    public DerivativeRepository(DatabaseContext context) : base(context)
    {
    }
}