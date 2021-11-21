using System.Collections.Generic;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.MqServices.Implementations;

namespace IM.Service.Company.Analyzer.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<Report> reportRepository,
        RepositorySet<Price> priceRepository) : base(
        new Dictionary<QueueExchanges, IRabbitActionService>
        {
            { QueueExchanges.Sync, new RabbitSyncService(companyRepository) },
            { QueueExchanges.Transfer, new RabbitTransferService(reportRepository, priceRepository) }
        })
    { }
}