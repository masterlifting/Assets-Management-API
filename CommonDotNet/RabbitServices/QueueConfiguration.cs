using CommonServices.RabbitServices.Configuration;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CommonServices.RabbitServices
{
    public static class QueueConfiguration
    {
        public static QueueExchange[] Exchanges => new QueueExchange[]
        {
            new QueueExchange (QueueExchanges.crud)
            {
                Queues = new Queue[]
                {
                    new Queue(QueueNames.companiesreports,false,true)
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
                    },
                    new Queue(QueueNames.companiesprices,false,true)
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
                            }
                        }
                    },
                    new Queue(QueueNames.companiesanalyzer,false,true)
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
                            }
                        }

                    }
                }
            },
            new QueueExchange (QueueExchanges.loader)
            {
                Queues = new Queue[]
                {
                    new Queue(QueueNames.companiesreports,false,true)
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
                    },
                    new Queue(QueueNames.companiesprices,false,true)
                    {
                        Params = new QueueParam[]
                        {
                            new QueueParam(QueueEntities.price)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.download
                                }
                            }
                        }
                    }
                }
            },
            new QueueExchange (QueueExchanges.calculator)
            {
                Queues = new Queue[]
                {
                    new Queue(QueueNames.companiesanalyzer)
                    {

                    }
                }
            }
        };
        public static (QueueExchange[] exchanges, Queue[] queues) GetConfiguredData(QueueExchanges[] ex, QueueNames[] qn)
        {
            var exchanges = Exchanges.Where(x => ex.Contains(x.NameEnum)).Distinct(new ExchangeComparer()).ToArray();
            var queues = exchanges.SelectMany(x => x.Queues).Where(x => qn.Contains(x.NameEnum)).Distinct(new QueueComparer()).ToArray();
            
            return (exchanges, queues);
        }
    }

    public enum QueueNames
    {
        companiesreports,
        companiesprices,
        companiesanalyzer,
    }
    public enum QueueExchanges
    {
        crud,
        loader,
        calculator
    }
    public enum QueueEntities
    {
        ticker,
        report,
        reportsource,
        price
    }
    public enum QueueActions
    {
        create,
        update,
        delete,
        download,
        calculate
    }
}