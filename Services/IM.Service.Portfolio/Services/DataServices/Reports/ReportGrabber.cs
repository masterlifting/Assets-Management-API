using IM.Service.Portfolio.DataAccess.Entities;
using IM.Service.Portfolio.DataAccess.Repositories;
using IM.Service.Portfolio.Services.DataServices.Reports.Implementations;

using Microsoft.Extensions.Logging;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.DataServices.Reports;

public class ReportGrabber : DataGrabber
{
    public ReportGrabber(
        Repository<Account> accountRepo,
        Repository<Derivative> derivativeRepo,
        Repository<Deal> dealRepo,
        Repository<Event> eventRepo,
        Repository<Report> reportRepo,
        ILogger<ReportGrabber> logger)
        : base(new()
        {
            { Brokers.Bcs, new BcsGrabber(accountRepo, derivativeRepo, dealRepo, eventRepo, reportRepo, logger) }
        })
    {
    }
}