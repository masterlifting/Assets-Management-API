using CommonServices.RepositoryService;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class AnalyzerRepository<T> : Repository<T, AnalyzerContext> where T : class
    {
        public AnalyzerRepository(AnalyzerContext context, IRepository<T> handler) : base(context, handler) { }
    }
}
