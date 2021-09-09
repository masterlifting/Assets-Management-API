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
                    new Queue(QueueNames.CompaniesReports,false,true)
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
                    new Queue(QueueNames.CompaniesPrices,false,true)
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
                    new Queue(QueueNames.CompaniesAnalyzer,false,true)
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
            new QueueExchange (QueueExchanges.Loader)
            {
                Queues = new[]
                {
                    new Queue(QueueNames.CompaniesReports,false,true)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Report)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Download
                                }
                            }
                        }
                    },
                    new Queue(QueueNames.CompaniesPrices,false,true)
                    {
                        Params = new[]
                        {
                            new QueueParam(QueueEntities.Price)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Download
                                }
                            }
                        }
                    }
                }
            },
            new QueueExchange (QueueExchanges.Calculator)
            {
                Queues = new[]
                {
                    new Queue(QueueNames.CompaniesAnalyzer,false)
                    {
                         Params = new[]
                        {
                            new QueueParam(QueueEntities.Price)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Calculate
                                }
                            },
                            new QueueParam(QueueEntities.Report)
                            {
                                Actions = new[]
                                {
                                    QueueActions.Calculate
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
        CompaniesReports,
        CompaniesPrices,
        CompaniesAnalyzer,
    }
    public enum QueueExchanges
    {
        Crud,
        Loader,
        Calculator
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
        Download,
        Calculate
    }
}