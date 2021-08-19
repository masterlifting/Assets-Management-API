using CommonServices.RabbitServices;

using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.CompaniesReports;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.Ticker;

using System.Collections.Generic;
using System.Text.Json;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitServices
{
    public class RabbitCrudService
    {
        private readonly RabbitService rabbitService;
        public RabbitCrudService(RabbitService rabbitService) => this.rabbitService = rabbitService;

        public void CreateCompany(CompanyModel company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTicker { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTicker { Name = company.Ticker });
            var reportSourceData = new List<string>(company.ReportSources.Length);

            var tickerData = new Dictionary<QueueNames, string>()
            {
                {QueueNames.companiesreportscrud,companiesReportsTicker },
                {QueueNames.companiespricescrud,companiesPricesTicker },
                {QueueNames.companiesanalyzercrud,analyzerTicker }
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
                rabbitService.GetPublisher(QueueExchanges.crud)
                                   .PublishTask(data.Key, QueueEntities.ticker, QueueActions.create, data.Value);

            rabbitService.GetPublisher(QueueExchanges.crud)
                               .PublishTask(QueueNames.companiesreportscrud, QueueEntities.reportsource, QueueActions.create, reportSourceData.ToArray());
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

            rabbitService.GetPublisher(QueueExchanges.crud)
                               .PublishTask(QueueNames.companiespricescrud, QueueEntities.ticker, QueueActions.update, companiesPricesTicker);

            rabbitService.GetPublisher(QueueExchanges.crud)
                               .PublishTask(QueueNames.companiesreportscrud, QueueEntities.reportsource, QueueActions.update, reportSourceData.ToArray());
        }
        public void DeleteCompany(string ticker) => rabbitService.GetPublisher(QueueExchanges.crud).PublishTask(QueueEntities.ticker, QueueActions.delete, ticker);
    }
}
