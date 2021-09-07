using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.Models.Dto.CompaniesPricesService;
using CommonServices.Models.Dto.CompaniesReportsService;
using CommonServices.RabbitServices;

using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Settings;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Text.Json;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitServices
{
    public class RabbitCrudService
    {
        private readonly RabbitPublisher publisher;
        public RabbitCrudService(IOptions<ServiceSettings> options) =>
            publisher = new RabbitPublisher(options.Value.ConnectionStrings.Mq, QueueExchanges.crud);

        public void CreateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.companiesreports,companiesReportsTicker },
                {QueueNames.companiesprices,companiesPricesTicker },
                {QueueNames.companiesanalyzer,analyzerTicker }
            };

            foreach (var data in tickerData)
                publisher.PublishTask(data.Key, QueueEntities.ticker, QueueActions.create, data.Value);
        }
        public void UpdateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            publisher.PublishTask(QueueNames.companiesanalyzer, QueueEntities.ticker, QueueActions.update, analyzerTicker);
            publisher.PublishTask(QueueNames.companiesprices, QueueEntities.ticker, QueueActions.update, companiesPricesTicker);
            publisher.PublishTask(QueueNames.companiesreports, QueueEntities.ticker, QueueActions.update, companiesReportsTicker);
        }
        public void DeleteCompany(string ticker) => publisher.PublishTask(new QueueNames[]
        {
            QueueNames.companiesreports,
            QueueNames.companiesprices,
            QueueNames.companiesanalyzer
        }
        , QueueEntities.ticker, QueueActions.delete, ticker);
    }
}
