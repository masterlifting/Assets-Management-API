using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Services.DataServices.Reports.Implementations;

using Microsoft.Extensions.Logging;

namespace IM.Service.Broker.Data.Services.DataServices.Reports;

public class ReportGrabber : DataGrabber
{
    public ReportGrabber(Repository<Report> reportRepository, Repository<Transaction> transactionRepository, Repository<Stock> stockRepository, ILogger<ReportGrabber> logger)
        : base(new()
        {
            { (byte)Enums.Brokers.Bcs, new BcsGrabber(reportRepository, transactionRepository, stockRepository, logger) }
        })
    {
    }
}