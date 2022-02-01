using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Services.DataServices.Reports.Implementations;

using Microsoft.Extensions.Logging;

using static IM.Service.Broker.Data.Enums;

namespace IM.Service.Broker.Data.Services.DataServices.Reports;

public class ReportGrabber : DataGrabber
{
    public ReportGrabber(
        Repository<Report> reportRepository, 
        Repository<Stock> stockRepository,
        Repository<Transaction> transactionRepository,
        Repository<Account> accountRepository,
        ILogger<ReportGrabber> logger)
        : base(new()
        {
            { Brokers.Bcs, new BcsGrabber(reportRepository, stockRepository, transactionRepository, accountRepository, logger) }
        })
    {
    }
}