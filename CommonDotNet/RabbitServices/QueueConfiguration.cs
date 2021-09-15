using System.Collections.Generic;
using CommonServices.RabbitServices.Configuration;

namespace CommonServices.RabbitServices
{
    public static class QueueConfiguration
    {
        public static IEnumerable<QueueExchange> Exchanges => new[]
        {
            new QueueExchange (QueueExchanges.Crud)
            {
                Queues = new[]
                {
                    new Queue(QueueNames.CompanyReports)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Ticker)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Create,
                                    QueueActions.Update,
                                    QueueActions.Delete
                                }
                            }
                        }
                    },
                    new Queue(QueueNames.CompanyPrices)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Ticker)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Create,
                                    QueueActions.Update,
                                    QueueActions.Delete
                                }
                            }
                        }
                    },
                    new Queue(QueueNames.CompanyAnalyzer)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Ticker)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Create,
                                    QueueActions.Update,
                                    QueueActions.Delete
                                }
                            }
                        }
                    }
                }
            },
            new QueueExchange (QueueExchanges.Data)
            {
                Queues = new[]
                {
                    new Queue(QueueNames.CompanyReports)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Report)
                            {
                                Actions = new[]
                                {
                                    QueueActions.GetData
                                }
                            }
                        }
                    },
                    new Queue(QueueNames.CompanyPrices)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Price)
                            {
                                Actions = new[]
                                {
                                    QueueActions.GetData
                                }
                            }
                        }
                    }
                }
            },
            new QueueExchange (QueueExchanges.Logic)
            {
                Queues = new[]
                {
                    new Queue(QueueNames.CompanyAnalyzer)
                    {
                         Params = new[]
                        {
                            new QueueParam(QueueEntities.Price)
                            {
                                Actions = new[]
                                {
                                    QueueActions.GetLogic
                                }
                            },
                            new QueueParam(QueueEntities.Report)
                            {
                                Actions = new[]
                                {
                                    QueueActions.GetLogic
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
        CompanyReports,
        CompanyPrices,
        CompanyAnalyzer,
        BrokerReports,
        BrokerAggregator
    }
    public enum QueueExchanges
    {
        Crud,
        Data,
        Logic
    }
    public enum QueueEntities
    {
        Ticker,
        Report,
        Price
    }
    public enum QueueActions
    {
        Create,
        Update,
        Delete,
        GetData,
        GetLogic
    }
}