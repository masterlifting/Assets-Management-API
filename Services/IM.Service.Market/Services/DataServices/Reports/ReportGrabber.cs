using Microsoft.Extensions.Logging;

using System;
using IM.Service.Market.Clients;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Services.DataServices.Reports.Implementations;

namespace IM.Service.Market.Services.DataServices.Reports;

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