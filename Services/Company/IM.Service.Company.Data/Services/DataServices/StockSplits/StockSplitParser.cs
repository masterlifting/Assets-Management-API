using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.StockSplits.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits;

public class StockSplitParser
{
    private readonly Dictionary<string, IStockSplitParser> parser;
    public StockSplitParser() =>
        parser = new(StringComparer.InvariantCultureIgnoreCase);

    public bool IsSource(string source) => parser.ContainsKey(source);
    public async Task<StockSplit[]> GetStockSplitsAsync(string source, DateDataConfigModel config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetStockSplitsAsync(source, config)
            : Array.Empty<StockSplit>();
    public async Task<StockSplit[]> GetStockSplitsAsync(string source, IEnumerable<DateDataConfigModel> config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetStockSplitsAsync(source, config)
            : Array.Empty<StockSplit>();
}