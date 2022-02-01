﻿using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Common.Net.RepositoryService;

namespace IM.Service.Broker.Data.DataAccess.Repository;

public class StockRepository : RepositoryHandler<Stock, DatabaseContext>
{
    public StockRepository(DatabaseContext context) : base(context)
    {
    }
}