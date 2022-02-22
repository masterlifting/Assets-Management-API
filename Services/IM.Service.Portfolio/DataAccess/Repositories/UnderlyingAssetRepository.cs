using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class UnderlyingAssetRepository : RepositoryHandler<UnderlyingAsset, DatabaseContext>
{
    public UnderlyingAssetRepository(DatabaseContext context) : base(context)
    {
    }
}