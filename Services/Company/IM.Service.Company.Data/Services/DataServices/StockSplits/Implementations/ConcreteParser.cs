using IM.Service.Common.Net;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.StockSplits.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits.Implementations;

public class ConcreteParser : IStockSplitParser
{
    private readonly ILogger<StockSplitParser> logger;

    public async Task<StockSplit[]> GetStockSplitsAsync(string source, DateDataConfigModel config)
    {
        var result = Array.Empty<StockSplit>();

        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException($"source value for '{config.CompanyId}' is null");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "investing parser error: {error}", exception.InnerException?.Message ?? exception.Message);
        }

        return result;
    }
    public async Task<StockSplit[]> GetStockSplitsAsync(string source, IEnumerable<DateDataConfigModel> config)
    {
        var _config = config.ToArray();
        var result = new List<StockSplit>(_config.Length * 5);

        foreach (var item in _config)
        {
            result.AddRange(await GetStockSplitsAsync(source, item));
            await Task.Delay(5000);
        }

        return result.ToArray();
    }
}