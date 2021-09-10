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
            publisher = new RabbitPublisher(options.Value.ConnectionStrings.Mq, QueueExchanges.Crud);

        public void CreateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker! });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker!, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker!, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.CompaniesReports,companiesReportsTicker },
                {QueueNames.CompaniesPrices,companiesPricesTicker },
                {QueueNames.CompaniesAnalyzer,analyzerTicker }
            };

            foreach (var (key, value) in tickerData)
                publisher.PublishTask(key, QueueEntities.Ticker, QueueActions.Create, value);
        }
        public void UpdateCompany(CompanyPostDto company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTickerDto { Name = company.Ticker! });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTickerDto { Name = company.Ticker!, SourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTickerDto { Name = company.Ticker!, SourceTypeId = company.ReportSourceTypeId, SourceValue = company.ReportSourceValue });

            publisher.PublishTask(QueueNames.CompaniesAnalyzer, QueueEntities.Ticker, QueueActions.Update, analyzerTicker);
            publisher.PublishTask(QueueNames.CompaniesPrices, QueueEntities.Ticker, QueueActions.Update, companiesPricesTicker);
            publisher.PublishTask(QueueNames.CompaniesReports, QueueEntities.Ticker, QueueActions.Update, companiesReportsTicker);
        }
        public void DeleteCompany(string ticker) => publisher.PublishTask(new QueueNames[]
        {
            QueueNames.CompaniesReports,
            QueueNames.CompaniesPrices,
            QueueNames.CompaniesAnalyzer
        }
        , QueueEntities.Ticker, QueueActions.Delete, ticker);
    }
}
