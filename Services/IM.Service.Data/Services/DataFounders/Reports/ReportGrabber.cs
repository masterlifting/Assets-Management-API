using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Services.DataFounders.Reports.Implementations;

namespace IM.Service.Data.Services.DataFounders.Reports;

public class ReportGrabber : DataGrabber
{
    public ReportGrabber(Repository<Report> repository, ILogger<ReportGrabber> logger, InvestingClient investingClient)
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(Enums.Sources.Investing), new InvestingGrabber(repository, logger, investingClient) }
        })
    {
    }
}