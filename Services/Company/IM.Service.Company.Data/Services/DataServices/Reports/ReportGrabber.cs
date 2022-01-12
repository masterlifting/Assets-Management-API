﻿using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.DataServices.Reports.Implementations;

using Microsoft.Extensions.Logging;

using System;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.Reports;

public class ReportGrabber : DataGrabber
{
    public ReportGrabber(Repository<Report> repository, ILogger<ReportGrabber> logger, InvestingClient investingClient)
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(SourceTypes.Investing), new InvestingGrabber(repository, logger, investingClient) }
        })
    {
    }
}