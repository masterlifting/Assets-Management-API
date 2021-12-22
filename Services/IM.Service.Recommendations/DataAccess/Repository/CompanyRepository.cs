using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.DataAccess.Entities;

namespace IM.Service.Recommendations.DataAccess.Repository;

public class CompanyRepository : RepositoryHandler<Company, DatabaseContext>
{
    public CompanyRepository(DatabaseContext context) : base(context)
    {
    }
}