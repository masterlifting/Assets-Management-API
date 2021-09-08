using CommonServices.RepositoryService;

namespace IM.Services.Companies.Prices.Api.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, PricesContext> where T : class
    {
        public RepositorySet(PricesContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
