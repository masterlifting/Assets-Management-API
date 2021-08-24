using CommonServices.RabbitServices;

using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.CompaniesReports;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.Ticker;
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

        public void CreateCompany(CompanyModel company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTicker { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTicker { Name = company.Ticker });
            var reportSourceData = new List<string>(company.ReportSources.Length);

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.companiesreports,companiesReportsTicker },
                {QueueNames.companiesprices,companiesPricesTicker },
                {QueueNames.companiesanalyzer,analyzerTicker }
            };

            foreach (var reportSource in company.ReportSources)
                reportSourceData.Add(JsonSerializer.Serialize(new CompaniesReportsReportSource
                {
                    IsActive = reportSource.IsActive,
                    ReportSourceTypeId = reportSource.ReportSourceTypeId,
                    Value = reportSource.Value,
                    TickerName = company.Ticker
                }));

            foreach (var data in tickerData)
                publisher.PublishTask(data.Key, QueueEntities.ticker, QueueActions.create, data.Value);

            publisher.PublishTask(QueueNames.companiesreports, QueueEntities.reportsource, QueueActions.create, reportSourceData.ToArray());
        }
        public void UpdateCompany(CompanyModel company)
        {
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });
            var reportSourceData = new List<string>(company.ReportSources.Length);

            foreach (var reportSource in company.ReportSources)
                reportSourceData.Add(JsonSerializer.Serialize(new CompaniesReportsReportSource
                {
                    Id = reportSource.Id,
                    IsActive = reportSource.IsActive,
                    ReportSourceTypeId = reportSource.ReportSourceTypeId,
                    Value = reportSource.Value,
                    TickerName = company.Ticker
                }));

            publisher.PublishTask(QueueNames.companiesprices, QueueEntities.ticker, QueueActions.update, companiesPricesTicker);

            publisher.PublishTask(QueueNames.companiesreports, QueueEntities.reportsource, QueueActions.update, reportSourceData.ToArray());
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
