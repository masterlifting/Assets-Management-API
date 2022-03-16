using IM.Service.MarketData.Clients;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.ManyToMany;
using IM.Service.MarketData.Services.DataLoaders.Reports.Implementations;
using static IM.Service.Common.Net.Helper;

namespace IM.Service.MarketData.Services.DataLoaders.Reports;

public class ReportLoader : DataLoader<Report>
{
    public ReportLoader(
        ILogger<DataLoader<Report>> logger,
        Repository<Report> repository,
        Repository<CompanySource> companySourceRepo,
        InvestingClient investingClient)
        : base(logger, repository, companySourceRepo, new Dictionary<byte, IDataGrabber>()
            {
                { (byte)Enums.Sources.Investing, new InvestingGrabber(repository, logger, investingClient) }
            })
    {
        IsCurrentDataCondition = x => IsMissingLastQuarter(x.Year, x.Quarter);
        TimeAgo = 3;
    }

    private static bool IsMissingLastQuarter(int year, byte quarter)
    {
        var (controlYear, controlQuarter) = QuarterHelper.SubtractQuarter(DateOnly.FromDateTime(DateTime.UtcNow));
        return controlYear > year || controlYear == year && controlQuarter > quarter;
    }
}