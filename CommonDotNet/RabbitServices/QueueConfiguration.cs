using CommonServices.RabbitServices.Configuration;

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
                    new Queue(QueueNames.companiesanalyzer,false,false)
                    {
                         Params = new QueueParam[]
                        {
                            new QueueParam(QueueEntities.price)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.calculate
                                }
                            },
                            new QueueParam(QueueEntities.report)
                            {
                                Actions = new QueueActions[]
                                {
                                    QueueActions.calculate
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