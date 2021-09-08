using CommonServices.RepositoryService;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, ReportsContext> where T : class
    {
        public RepositorySet(ReportsContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
