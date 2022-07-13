using IM.Service.Shared.Models.RabbitMq;

using System.Collections.Generic;

namespace IM.Service.Shared.RabbitMq;

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
                        new QueueEntity(QueueEntities.Asset)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Set
                            }
                        },
                        new QueueEntity(QueueEntities.Assets)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Set
                            }
                        }
                    }
                },
                new Queue(QueueNames.Recommendations)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Asset)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Assets)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete,
                                QueueActions.Set
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
                new Queue(QueueNames.Recommendations)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Ratings)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Deals)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute,
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Cost)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Costs)
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
        new QueueExchange (QueueExchanges.Function)
        {
            Queues = new[]
            {
                new Queue(QueueNames.Market)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Cost)
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
                        new QueueEntity(QueueEntities.Costs)
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
                        new QueueEntity(QueueEntities.AssetSource)
                        {
                            Actions = new[]
                            {
                                QueueActions.Get
                            }
                        },
                        new QueueEntity(QueueEntities.AssetSources)
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
                        },
                        new QueueEntity(QueueEntities.Deal)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Deals)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Event)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Events)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Balance)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Balances)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.Update,
                                QueueActions.Delete
                            }
                        },
                        new QueueEntity(QueueEntities.Derivative)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute
                            }
                        },
                        new QueueEntity(QueueEntities.Derivatives)
                        {
                            Actions = new[]
                            {
                                QueueActions.Compute
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
    Market,
    Portfolio,
    Recommendations
}
public enum QueueExchanges
{
    Sync,
    Transfer,
    Function
}
public enum QueueEntities
{
    Asset,
    Assets,
    AssetSource,
    AssetSources,
    Report,
    Reports,
    Split,
    Splits,
    Float,
    Floats,
    Cost,
    Costs,
    Dividend,
    Dividends,
    Rating,
    Ratings,
    Coefficient,
    Coefficients,
    Deal,
    Deals,
    Event,
    Events,
    Balance,
    Balances,
    Derivative,
    Derivatives
}
public enum QueueActions
{
    Create,
    Update,
    Delete,
    Get,
    Set,
    Compute
}