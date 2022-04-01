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
        IsCurrentDataCondition = x => IsMissingLastQuarter(x.Year, x.Quarter);
        TimeAgo = 3;
    }

    private static bool IsMissingLastQuarter(int year, byte quarter)
    {
        var (controlYear, controlQuarter) = QuarterHelper.SubtractQuarter(DateOnly.FromDateTime(DateTime.UtcNow));
        return controlYear > year || controlYear == year && controlQuarter > quarter;
    }
}