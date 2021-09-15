using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.RabbitServices;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Text.Json;
using IM.Gateway.Companies.Models.Dto;
using IM.Gateway.Companies.Settings;

namespace IM.Gateway.Companies.Services.RabbitServices
{
    public class RabbitCrudService
    {
        private readonly RabbitPublisher publisher;
        public RabbitCrudService(IOptions<ServiceSettings> options) =>
            publisher = new RabbitPublisher(options.Value.ConnectionStrings.Mq, QueueExchanges.Crud);

        public void CreateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker! });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker!, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker!, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.CompanyReports,companiesReportsTicker },
                {QueueNames.CompanyPrices,companiesPricesTicker },
                {QueueNames.CompanyAnalyzer,analyzerTicker }
            };

            foreach (var (key, value) in tickerData)
                publisher.PublishTask(key, QueueEntities.Ticker, QueueActions.Create, value);
        }
        public void UpdateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker! });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker!, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker!, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            publisher.PublishTask(QueueNames.CompanyAnalyzer, QueueEntities.Ticker, QueueActions.Update, analyzerTicker);
            publisher.PublishTask(QueueNames.CompanyPrices, QueueEntities.Ticker, QueueActions.Update, companiesPricesTicker);
            publisher.PublishTask(QueueNames.CompanyReports, QueueEntities.Ticker, QueueActions.Update, companiesReportsTicker);
        }
        public void DeleteCompany(string ticker) => publisher.PublishTask(new QueueNames[]
        {
            QueueNames.CompanyReports,
            QueueNames.CompanyPrices,
            QueueNames.CompanyAnalyzer
        }
        , QueueEntities.Ticker, QueueActions.Delete, ticker);
    }
}
