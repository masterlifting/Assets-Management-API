using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class ReportRepository : RepositoryHandler<Report, DatabaseContext>
{
    public ReportRepository(DatabaseContext context) : base(context)
    {
    }
}