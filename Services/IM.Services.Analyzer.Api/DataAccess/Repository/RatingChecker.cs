using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class RatingChecker : IEntityChecker<Rating>
    {
        private readonly AnalyzerContext context;
        public RatingChecker(AnalyzerContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(Rating rating) => await context.Ratings.FindAsync(rating.Place) is not null;
        public bool WithError(Rating rating) => false;
    }
}
