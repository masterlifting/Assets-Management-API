using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class RatingRepositoryHandler : RepositoryHandler<Rating, DatabaseContext>
{
    public RatingRepositoryHandler(DatabaseContext context) : base(context)
    {
    }
}