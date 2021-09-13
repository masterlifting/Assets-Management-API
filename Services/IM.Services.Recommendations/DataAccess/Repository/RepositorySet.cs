using CommonServices.RepositoryService;

namespace IM.Services.Recommendations.DataAccess.Repository
{
    public class RepositorySet<T> : Repository<T, AnalyzerContext> where T : class
    {
        public RepositorySet(AnalyzerContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
