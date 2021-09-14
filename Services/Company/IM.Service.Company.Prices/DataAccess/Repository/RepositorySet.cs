using CommonServices.RepositoryService;

namespace IM.Service.Company.Prices.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, DatabaseContext> where T : class
    {
        public RepositorySet(DatabaseContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
