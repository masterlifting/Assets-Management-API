using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.DataLoaders.Reports.Implementations;

using static IM.Service.Common.Net.Helper;

namespace IM.Service.Market.Services.DataLoaders.Reports;

public class ReportLoader : DataLoader<Report>
{
    public ReportLoader(
        ILogger<DataLoader<Report>> logger,
        Repository<Report> repository,
        InvestingClient investingClient)
        : base(logger, repository, new Dictionary<byte, IDataGrabber>
        {
                { (byte)Enums.Sources.Investing, new InvestingGrabber(repository, logger, investingClient) }
        })
    {
        IsCurrentDataCondition = x => IsCurrentData(x.Year, x.Quarter);
        TimeAgo = 3;
    }

    private static bool IsCurrentData(int year, byte quarter)
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentQuarter = QuarterHelper.GetQuarter(DateTime.UtcNow.Month);

        (currentYear, currentQuarter) = QuarterHelper.SubtractQuarter(currentYear, currentQuarter);

        return currentYear == year && currentQuarter == quarter;
    }
}