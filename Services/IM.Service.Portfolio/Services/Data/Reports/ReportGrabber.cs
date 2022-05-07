using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.Data.Reports.Implementations;

using Microsoft.Extensions.Logging;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Data.Reports;

public sealed class ReportGrabber : DataGrabber
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