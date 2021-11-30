using IM.Service.Common.Net.RabbitServices.Configuration;

using System.Collections.Generic;

namespace IM.Service.Common.Net.RabbitServices;

public static class QueueConfiguration
{
    public static IEnumerable<QueueExchange> Exchanges => new[]
    {
        new QueueExchange (QueueExchanges.Sync)
        {
            Queues = new[]
            {
                new Queue(QueueNames.CompanyData)
                {
                    Params = new[]
                    {
                        new QueueParam(QueueEntities.Company)
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
                        new QueueParam(QueueEntities.Company)
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
        new QueueExchange (QueueExchanges.Transfer)
        {
            Queues = new[]
            {
                new Queue(QueueNames.CompanyAnalyzer)
                {
                    Params = new[]
                    {
                        new QueueParam(QueueEntities.CompanyReport)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create
                            }
                        },
                        new QueueParam(QueueEntities.Price)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create
                            }
                        },
                        new QueueParam(QueueEntities.Coefficient)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create
                            }
                        }

                    }
                },
                new Queue(QueueNames.Recommendation)
                {
                    Params = new[]
                    {
                        new QueueParam(QueueEntities.Rating)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create
                            }
                        }
                    }
                }
            }
        },
        new QueueExchange (QueueExchanges.Function)
        {
            Queues = new[]
            {
                new Queue(QueueNames.CompanyData)
                {
                    Params = new[]
                    {
                        new QueueParam(QueueEntities.Price)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueParam(QueueEntities.CompanyReport)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueParam(QueueEntities.StockSplit)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueParam(QueueEntities.StockVolume)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
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
    CompanyData,
    CompanyAnalyzer,
    Recommendation
}
public enum QueueExchanges
{
    Sync,
    Transfer,
    Function
}
public enum QueueEntities
{
    Company,
    CompanyReport,
    StockSplit,
    StockVolume,
    Price,
    Rating,
    Coefficient
}
public enum QueueActions
{
    Create,
    Update,
    Delete,
    Call
}