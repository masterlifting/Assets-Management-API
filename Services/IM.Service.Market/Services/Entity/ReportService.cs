using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Services.Helpers;

namespace IM.Service.Market.Services.Entity;

public sealed class ReportService : StatusChanger<Report>
{
    public DataLoader<Report> Loader { get; }

    public ReportService(Repository<Report> reportRepo, DataLoader<Report> loader) : base(reportRepo) => Loader = loader;
}