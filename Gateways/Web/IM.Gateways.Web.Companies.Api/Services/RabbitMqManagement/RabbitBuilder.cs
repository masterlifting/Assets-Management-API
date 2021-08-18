using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Connection;
using IM.Gateways.Web.Companies.Api.Settings.Mq;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement
{
    public class RabbitBuilder
    {
        private readonly ConnectionModel mqConnection;
        private readonly IConnection connection;

        public RabbitBuilder(IOptions<ServiceSettings> options)
        {
            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;
            Exchanges = options.Value.Exchanges;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            connection = factory.CreateConnection();
            Channel = connection.CreateModel();

            foreach (var exchange in Exchanges)
            {
                Channel.ExchangeDeclare(exchange.Name, exchange.Type);

                foreach (var queue in exchange.Queues)
                {
                    Channel.QueueDeclare(queue.Name, false, false, false, null);

                    foreach (var route in queue.Params)
                        Channel.QueueBind(queue.Name, exchange.Name, $"{queue.Name}.{route.Entity}.*");
                }
            }
        }

        public Exchange[] Exchanges { get; }
        public IModel Channel { get; }
    }
}
