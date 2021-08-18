
using CommonServices.Models;
using CommonServices.ParserServices;
using CommonServices.RabbitServices.Configuration;

using RabbitMQ.Client;

namespace CommonServices.RabbitServices
{
    public class RabbitBuilder
    {
        public RabbitBuilder(string connectionString)
        {
            var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();

            foreach (var exchange in QueueConfiguration.Exchanges)
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

        public IConnection Connection { get; }
        public IModel Channel { get; }
    }
}
