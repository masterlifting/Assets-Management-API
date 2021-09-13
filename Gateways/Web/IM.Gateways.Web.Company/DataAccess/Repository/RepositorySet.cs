using CommonServices.RepositoryService;

namespace IM.Gateways.Web.Company.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, GatewaysContext> where T : class
    {
        public RepositorySet(GatewaysContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
