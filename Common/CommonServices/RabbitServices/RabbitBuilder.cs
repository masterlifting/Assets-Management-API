
using CommonServices.Models;
using CommonServices.ParserServices;
using CommonServices.RabbitServices.Configuration;

using RabbitMQ.Client;

using System.Linq;

namespace CommonServices.RabbitServices
{
    public class RabbitBuilder
    {
        public RabbitBuilder(string connectionString, (QueueExchange[] exchanges, Queue[] queues) cfgData)
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

            foreach (var exchange in cfgData.exchanges)
            {
                Channel.ExchangeDeclare(exchange.NameString, exchange.Type);

                foreach (var queue in exchange.Queues.Join(cfgData.queues, x => x.NameString, y => y.NameString, (x, y) => x))
                {
                    Channel.QueueDeclare(queue.NameString, false, false, false, null);

                    foreach (var route in queue.Params)
                        Channel.QueueBind(queue.NameString, exchange.NameString, $"{queue.NameString}.{route.EntityNameString}.*");
                }
            }
            Exchanges = cfgData.exchanges;
            Queues = cfgData.queues;
        }

        public IConnection Connection { get; }
        public IModel Channel { get; }
        public QueueExchange[] Exchanges { get; }
        public Queue[] Queues { get; }
    }
}
