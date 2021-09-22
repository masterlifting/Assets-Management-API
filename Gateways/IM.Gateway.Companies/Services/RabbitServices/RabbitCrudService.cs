using CommonServices.RabbitServices;

using CommonServices.Models.Dto.GatewayCompanies;
using IM.Gateway.Companies.Settings;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Text.Json;

namespace IM.Gateway.Companies.Services.RabbitServices
{
    public class RabbitCrudService
    {
        private readonly RabbitPublisher publisher;
        public RabbitCrudService(IOptions<ServiceSettings> options) =>
            publisher = new RabbitPublisher(options.Value.ConnectionStrings.Mq, QueueExchanges.Crud);

        public void CreateCompany(CompanyPostDto company)
        {
            if (company.Ticker is null)
                return;

            var analyzerTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyAnalyzer.TickerPostDto
            {
                Name = company.Ticker
            });
            var priceTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyPrices.TickerPostDto
            {
                Name = company.Ticker,
                SourceTypeId = company.PriceSourceTypeId
            });
            var reportTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyReports.TickerPostDto
            {
                Name = company.Ticker,
                SourceTypeId = company.ReportSourceTypeId,
                SourceValue = company.ReportSourceValue
            });

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.CompanyReports,reportTicker },
                {QueueNames.CompanyPrices,priceTicker },
                {QueueNames.CompanyAnalyzer,analyzerTicker }
            };

            foreach (var (key, value) in tickerData)
                publisher.PublishTask(key, QueueEntities.Ticker, QueueActions.Create, value);
        }
        public void UpdateCompany(CompanyPostDto company)
        {
            if (company.Ticker is null)
                return;

            var analyzerTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyAnalyzer.TickerPostDto
            {
                Name = company.Ticker
            });
            var priceTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyPrices.TickerPostDto
            {
                Name = company.Ticker,
                SourceTypeId = company.PriceSourceTypeId
            });
            var reportTicker = JsonSerializer.Serialize(new CommonServices.Models.Dto.CompanyReports.TickerPostDto
            {
                Name = company.Ticker,
                SourceTypeId = company.ReportSourceTypeId,
                SourceValue = company.ReportSourceValue
            });

            publisher.PublishTask(QueueNames.CompanyAnalyzer, QueueEntities.Ticker, QueueActions.Update, analyzerTicker);
            publisher.PublishTask(QueueNames.CompanyPrices, QueueEntities.Ticker, QueueActions.Update, priceTicker);
            publisher.PublishTask(QueueNames.CompanyReports, QueueEntities.Ticker, QueueActions.Update, reportTicker);
        }
        public void DeleteCompany(string ticker) => publisher.PublishTask(new[]
        {
            QueueNames.CompanyReports,
            QueueNames.CompanyPrices,
            QueueNames.CompanyAnalyzer
        }
        , QueueEntities.Ticker, QueueActions.Delete, ticker);
    }
}
