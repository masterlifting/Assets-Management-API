using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.CompaniesReports;
using IM.Gateways.Web.Companies.Api.Models.Rabbit.Ticker;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Connection;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations
{
    public class RabbitmqManager : IRabbitmqManager
    {
        private readonly ConnectionModel mqConnection;

        private readonly IConnection connection;
        private readonly IModel channel;

        private readonly string queueName;

        public RabbitmqManager(IOptions<ServiceSettings> options)
        {
            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("crud", ExchangeType.Topic);
            queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "crud", "*.*.ticker");
            channel.QueueBind(queueName, "crud", "companiesreports.*.reportsource");
        }

        public async Task CreateCompanyAsync(CompanyModel company)
        {
            var analyzerTicker = JsonSerializer.Serialize(new AnalyzerTicker { Name = company.Ticker });
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });
            var companiesReportsTicker = JsonSerializer.Serialize(new CompaniesReportsTicker { Name = company.Ticker });

            await Task.WhenAll(new Task[]
            {
                Task.Run(() => channel.BasicPublish(
                "crud"
                , "companiesprices.create.ticker"
                , null
                , Encoding.UTF8.GetBytes(companiesPricesTicker))),

                Task.Run(() => channel.BasicPublish(
                "crud"
                , "companiesreports.create.ticker"
                , null
                , Encoding.UTF8.GetBytes(companiesReportsTicker))),

                Task.Run(() => channel.BasicPublish(
                "crud"
                , "analyzer.create.ticker"
                , null
                , Encoding.UTF8.GetBytes(analyzerTicker)))
            });

            CreateCompaniesReportsReportSource(company);

            void CreateCompaniesReportsReportSource(CompanyModel company)
            {
                foreach (var item in company.ReportSources)
                {
                    var reportSource = JsonSerializer.Serialize(new CompaniesReportsReportSource
                    {
                        IsActive = item.IsActive,
                        ReportSourceTypeId = item.ReportSourceTypeId,
                        Value = item.Value,
                        TickerName = company.Ticker
                    });

                    channel.BasicPublish(
                    "crud"
                    , "companiesreports.create.reportsource"
                    , null
                    , Encoding.UTF8.GetBytes(reportSource));
                }
            }
        }
        public async Task UpdateCompanyAsync(CompanyModel company)
        {
            var companiesPricesTicker = JsonSerializer.Serialize(new CompaniesPricesTicker { Name = company.Ticker, PriceSourceTypeId = company.PriceSourceTypeId });

            await Task.Run(() => channel.BasicPublish(
                 "crud"
                 , "companiesprices.update.ticker"
                 , null
                 , Encoding.UTF8.GetBytes(companiesPricesTicker)));


            foreach (var item in company.ReportSources)
            {
                var reportSource = JsonSerializer.Serialize(new CompaniesReportsReportSource
                {
                    Id = item.Id,
                    IsActive = item.IsActive,
                    ReportSourceTypeId = item.ReportSourceTypeId,
                    Value = item.Value,
                    TickerName = company.Ticker
                });

                channel.BasicPublish(
                "crud"
                , "companiesreports.update.reportsource"
                , null
                , Encoding.UTF8.GetBytes(reportSource));
            }
        }
        public async Task DeleteCompanyAsync(string ticker)
        {
            await Task.WhenAll(new Task[]
           {
                Task.Run(() => channel.BasicPublish(
                "crud"
                , "companiesprices.delete.ticker"
                , null
                , Encoding.UTF8.GetBytes(ticker))),

                Task.Run(() => channel.BasicPublish(
                "crud"
                , "companiesreports.delete.ticker"
                , null
                , Encoding.UTF8.GetBytes(ticker))),

                Task.Run(() => channel.BasicPublish(
                "crud"
                , "analyzer.delete.ticker"
                , null
                , Encoding.UTF8.GetBytes(ticker)))
           });
        }
    }
}
