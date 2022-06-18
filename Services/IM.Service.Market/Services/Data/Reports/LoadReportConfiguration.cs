using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Reports.Implementations;
using IM.Service.Market.Services.Tasks;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;
using static IM.Service.Shared.Helpers.LogicHelper;

namespace IM.Service.Market.Services.Data.Reports;

public sealed class LoadReportConfiguration : IDataLoaderConfiguration<Report>
{
    public Func<Report, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Report> Comparer { get; }
    public ILastDataHelper<Report> LastDataHelper { get; }
    public DataGrabber<Report> Grabber { get; }

    public LoadReportConfiguration(IOptions<ServiceSettings> options, Repository<Report> repository, InvestingClient investingClient)
    {
        var settings = options.Value.LoadData.Tasks.FirstOrDefault(x => x.Name.Equals(nameof(LoadReportTask), StringComparison.OrdinalIgnoreCase)) ?? new();

        Grabber = new(new()
        {
            {(byte) Enums.Sources.Investing, new InvestingGrabber(investingClient)}
        });
        IsCurrentDataCondition = x => IsCurrentData(x.Year, x.Quarter);
        Comparer = new DataQuarterComparer<Report>();
        LastDataHelper = new LastQuarterHelper<Report>(repository, settings.DaysAgo);
    }

    private static bool IsCurrentData(int year, byte quarter)
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentQuarter = QuarterHelper.GetQuarter(DateTime.UtcNow.Month);

        (currentYear, currentQuarter) = QuarterHelper.SubtractQuarter(currentYear, currentQuarter);

        return currentYear == year && currentQuarter == quarter;
    }
}