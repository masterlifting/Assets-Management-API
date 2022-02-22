
using System.Collections.Generic;

namespace IM.Service.Common.Net.RabbitServices.Configuration;

public static class QueueConfiguration
{
    public static IEnumerable<QueueExchange> Exchanges => new[]
    {
        new QueueExchange (QueueExchanges.Sync)
        {
            Queues = new[]
            {
                new Queue(QueueNames.MarketAnalyzer)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Company)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Companies)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.CreateUpdateDelete,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        }
                    }
                },
                new Queue(QueueNames.PortfolioData)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Company)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Companies)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.CreateUpdateDelete,
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
                new Queue(QueueNames.MarketAnalyzer)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Report)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.Reports)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.Price)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.Prices)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.Coefficient)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.Coefficients)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
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
                new Queue(QueueNames.MarketData)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Price)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.Prices)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.Report)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.Reports)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.StockSplit)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.StockSplits)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.StockVolume)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.StockVolumes)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        }
                    }
                },
                new Queue(QueueNames.PortfolioData)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Report)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.Reports)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        }
                    }
                },
                new Queue(QueueNames.MarketAnalyzer)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Rating)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.Ratings)
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
    MarketData,
    MarketAnalyzer,
    PortfolioData,
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
    Companies,
    Report,
    Reports,
    StockSplit,
    StockSplits,
    StockVolume,
    StockVolumes,
    Price,
    Prices,
    Rating,
    Ratings,
    Coefficient,
    Coefficients,
    Transaction,
    Transactions
}
public enum QueueActions
{
    Create,
    CreateUpdate,
    CreateUpdateDelete,
    Update,
    Delete,
    Call
}