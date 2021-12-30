
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
                new Queue(QueueNames.CompanyData)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Company)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.CreateUpdateDelete,
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
                new Queue(QueueNames.CompanyAnalyzer)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Company)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate,
                                QueueActions.CreateUpdateDelete,
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
                new Queue(QueueNames.CompanyAnalyzer)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.CompanyReport)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create,
                                QueueActions.CreateUpdate
                            }
                        },
                        new QueueEntity(QueueEntities.CompanyReports)
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
                },
                new Queue(QueueNames.Recommendation)
                {
                    Entities = new[]
                    {
                        new QueueEntity(QueueEntities.Rating)
                        {
                            Actions = new[]
                            {
                                QueueActions.Create
                            }
                        },
                        new QueueEntity(QueueEntities.Ratings)
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
                        new QueueEntity(QueueEntities.CompanyReport)
                        {
                            Actions = new[]
                            {
                                QueueActions.Call
                            }
                        },
                        new QueueEntity(QueueEntities.CompanyReports)
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
                new Queue(QueueNames.CompanyAnalyzer)
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
    Companies,
    CompanyReport,
    CompanyReports,
    StockSplit,
    StockSplits,
    StockVolume,
    StockVolumes,
    Price,
    Prices,
    Rating,
    Ratings,
    Coefficient,
    Coefficients
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