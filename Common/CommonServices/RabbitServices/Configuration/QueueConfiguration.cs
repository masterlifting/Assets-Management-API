using RabbitMQ.Client;

namespace CommonServices.RabbitServices.Configuration
{
    public static class QueueConfiguration
    {
        public static QueueExchange[] Exchanges => new QueueExchange[]
        {
            new QueueExchange (QueueExchanges.crud, ExchangeType.Topic)
            {
                Queues = new Queue[]
                {
                    new Queue(QueueNames.companiesreportscrud)
                    {
                        Params = new QueueParam[]
                        {
                            new QueueParam(QueueEntities.ticker)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.create,
                                    QueueActions.update,
                                    QueueActions.delete
                                }
                            },
                            new QueueParam(QueueEntities.reportsource)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.create,
                                    QueueActions.update
                                }
                            }
                        }
                    }
                }
            },
            new (QueueExchanges.parser, ExchangeType.Topic)
            {
                Queues = new Queue[]
                {
                    new Queue(QueueNames.companiesreportsparser)
                    {
                        Params = new QueueParam[]
                        {
                            new QueueParam(QueueEntities.report)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.download
                                }
                            }
                        }
                    }
                }
            }
        };
    }
    
    public enum QueueNames
    {
        companiesreportscrud,
        companiesreportsparser
    }
    public enum QueueActions
    {
        create,
        update,
        delete,
        download
    }
    public enum QueueEntities
    {
        ticker,
        report,
        reportsource
    }
    public enum QueueExchanges
    {
        crud,
        parser
    }
}
