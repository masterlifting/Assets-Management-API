using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.StockVolumes.Implementations;
using IM.Service.Company.Data.Services.DataServices.StockVolumes.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes;

public class StockVolumeParser
{
    private readonly Dictionary<string, IStockVolumeParser> parser;
    public StockVolumeParser(ILogger<StockVolumeParser> logger, InvestingClient investingClient) =>
        parser = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(SourceTypes.Investing), new InvestingParser(logger, investingClient) }
        };

    public bool IsSource(string source) => parser.ContainsKey(source);
    public async Task<StockVolume[]> GetStockVolumesAsync(string source, DateDataConfigModel config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetStockVolumesAsync(source, config)
            : Array.Empty<StockVolume>();
    public async Task<StockVolume[]> GetStockVolumesAsync(string source, IEnumerable<DateDataConfigModel> config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetStockVolumesAsync(source, config)
            : Array.Empty<StockVolume>();
}