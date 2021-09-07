using CommonServices.RepositoryService;

namespace IM.Services.Companies.Prices.Api.DataAccess.Repository
{
    public class PricesRepository<T> : Repository<T, PricesContext> where T : class
    {
        public PricesRepository(PricesContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
