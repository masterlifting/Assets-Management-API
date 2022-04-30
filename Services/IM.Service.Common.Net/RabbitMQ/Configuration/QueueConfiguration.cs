using System.Collections.Generic;

namespace IM.Service.Common.Net.RabbitMQ.Configuration;

public static class QueueConfiguration
{
    public static IEnumerable<QueueExchange> Exchanges => new[]
    {
        new QueueExchange (QueueExchanges.Sync)
        {
            Queues = new[]
            {
                new Queue(QueueNames.Portfolio)
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
        new QueueExchange (QueueExchanges.Transfer),
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
                                QueueActions.Get,
                                QueueActions.Set,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Prices)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Set,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Report)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Set,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Reports)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Set,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Split)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Splits)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Float)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Floats)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Dividend)
                        {
                            Actions = new[]
                            {
                                QueueActions.Set,
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.Dividends)
                        {
                            Actions = new[]
                            {
                                QueueActions.Set,
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.Coefficient)
                        {
                            Actions = new[]
                            {
                                QueueActions.Set,
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.Coefficients)
                        {
                            Actions = new[]
                            {
                                QueueActions.Set,
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.CompanySource)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.CompanySources)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.Rating)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute
                            }
                        },
                        new QueueEntity(QueueEntities.Ratings)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute
                            }
                        }
                    }
                },
                new Queue(QueueNames.Portfolio)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Report)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.Reports)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get
                            }
                        }
                    }
                },
            }
        }
    };
}

public enum QueueNames
{
    MarketData,
    Portfolio,
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
    CompanySource,
    CompanySources,
    Report,
    Reports,
    Split,
    Splits,
    Float,
    Floats,
    Price,
    Prices,
    Dividend,
    Dividends,
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
    Get,
    Set,
    Compute
}