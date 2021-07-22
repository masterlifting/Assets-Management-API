using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Connection;
using IM.Gateways.Web.Companies.Api.Settings.Mq;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System.Text;
using System.Text.Json;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations
{
    public class RabbitmqManager : IRabbitmqManager
    {
        private readonly ConnectionModel mqConnection;
        private readonly QueueModel queueCompaniesPrices;
        private readonly QueueModel queueCompaniesReports;
        private readonly QueueModel queueAnalyzer;

        private readonly IConnection connection;
        private readonly IModel channel;

        public RabbitmqManager(IOptions<ServiceSettings> options)
        {
            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;
            queueCompaniesPrices = new SettingsConverter<QueueModel>(options.Value.MqSettings.QueueCompaniesPrices).Model;
            queueCompaniesReports = new SettingsConverter<QueueModel>(options.Value.MqSettings.QueueCompaniesReports).Model;
            queueAnalyzer = new SettingsConverter<QueueModel>(options.Value.MqSettings.QueueAnalyzer).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            
            channel.QueueDeclare(
                    queueCompaniesPrices.Name
                    , false
                    , false
                    , false
                    , null);
            channel.QueueDeclare(
                   queueCompaniesReports.Name
                    , false
                    , false
                    , false
                    , null);
            channel.QueueDeclare(
                    queueAnalyzer.Name
                    , false
                    , false
                    , false
                    , null);
        }
    
        public bool CreateCompany(CompanyModel company)
        {
            bool result = false;

            if (company.PriceSourceTypeId.HasValue)
            {
                var ticker = JsonSerializer.Serialize(new CompaniesPricesTicker
                {
                    Name = company.Ticker,
                    PriceSourceTypeId = company.PriceSourceTypeId.Value
                });

                var body = Encoding.UTF8.GetBytes(ticker);

                channel.BasicPublish(
                    ""
                    , queueCompaniesPrices.Name
                    , null
                    , body);
                channel.BasicPublish(
                    ""
                    , queueCompaniesReports.Name
                    , null
                    , body);
                channel.BasicPublish(
                    ""
                    , queueAnalyzer.Name
                    , null
                    , body);

                result = true;
            }

            return result;
        }
        public bool EditCompany(CompanyModel company)
        {
            throw new System.NotImplementedException();
        }
        public bool DeleteCompany(string ticker)
        {
            throw new System.NotImplementedException();
        }
    }
}
