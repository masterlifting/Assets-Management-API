using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.CompaniesReports;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.Ticker;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations
{
    public class CrudExchange : ICrudExchange
    {
        private readonly RabbitAction queueAction;

        private const string companiesReports = "companiesreports";
        private const string companiesPrices = "companiesprices";
        private const string analyzer = "analyzer";

        private const string create = "create";
        private const string update = "update";
        private const string delete = "delete";

        public CrudExchange(RabbitBuilder builder)
        {
            var exchange = builder.Exchanges.FirstOrDefault(x => x.Name.Equals("crud", StringComparison.OrdinalIgnoreCase));

            if (exchange is null)
                throw new NullReferenceException("Exchange not found!");

            queueAction = new RabbitAction(builder.Channel, exchange);
        }

        public void CreateCompany(CompanyModel company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTicker { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTicker { Name = company.Ticker });
            var reportSourceData = new List<string>(company.ReportSources.Length);

            var tickerData = new Dictionary<string, string>()
            {
                {companiesReports,companiesReportsTicker },
                {companiesPrices,companiesPricesTicker },
                {analyzer,analyzerTicker }
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
                queueAction.SetQueueTask(data.Key, "ticker", create, data.Value);

            queueAction.SetQueueTask(companiesReports, "reportsource", create, reportSourceData);
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

            queueAction.SetQueueTask(companiesPrices, "ticker", update, companiesPricesTicker);
            queueAction.SetQueueTask(companiesReports, "reportsource", update, reportSourceData);
        }
        public void DeleteCompany(string ticker) => queueAction.SetQueueTask("ticker", delete, ticker);
    }
}
