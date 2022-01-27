using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class ReportRepository : RepositoryHandler<Report, DatabaseContext>
{
    public ReportRepository(DatabaseContext context) : base(context)
    {
    }
}