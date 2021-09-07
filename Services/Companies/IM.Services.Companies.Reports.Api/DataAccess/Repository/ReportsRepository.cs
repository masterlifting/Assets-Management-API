using CommonServices.RepositoryService;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class ReportsRepository<T> : Repository<T, ReportsContext> where T : class
    {
        public ReportsRepository(ReportsContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
