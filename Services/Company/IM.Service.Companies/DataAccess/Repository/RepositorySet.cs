using CommonServices.RepositoryService;

namespace IM.Service.Companies.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, DatabaseContext> where T : class
    {
        public RepositorySet(DatabaseContext context, IRepositoryHandler<T> handler) : base(context, handler) { }
    }
}
