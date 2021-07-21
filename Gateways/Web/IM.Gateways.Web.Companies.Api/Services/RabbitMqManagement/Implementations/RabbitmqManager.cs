using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Rabbit;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Rabbitmq;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System.Text;
using System.Text.Json;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations
{
    public class RabbitmqManager : IRabbitmqManager
    {
        private readonly RabbitmqSettings settingsRabbitmq;
        private readonly IConnection connection;
        private readonly IModel channel;

        public RabbitmqManager(IOptions<ServiceSettings> options)
        {
            settingsRabbitmq = options.Value.RabbitmqSettings;

            var factory = new ConnectionFactory() { HostName = settingsRabbitmq.Host, UserName = settingsRabbitmq.UserName, Password = settingsRabbitmq.Password };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(
                    settingsRabbitmq.QueueCompaniesPrices.Name
                    , false
                    , false
                    , false
                    , null);
            channel.QueueDeclare(
                   settingsRabbitmq.QueueCompaniesReports.Name
                    , false
                    , false
                    , false
                    , null);
            channel.QueueDeclare(
                    settingsRabbitmq.QueueAnalyzer.Name
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
                    , settingsRabbitmq.QueueCompaniesPrices.Name
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
