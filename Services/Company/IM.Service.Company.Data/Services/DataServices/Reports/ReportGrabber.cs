using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Reports.Implementations;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.Reports;

public class ReportGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    public ReportGrabber(Repository<Report> repository, ILogger<ReportGrabber> logger, InvestingClient investingClient) =>
        grabber = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(SourceTypes.Investing), new InvestingGrabber(repository, logger, investingClient) }
        };

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentReportsAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabCurrentReportsAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, configs)
            : Task.CompletedTask;

    public Task GrabHistoryReportsAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabHistoryReportsAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, configs)
            : Task.CompletedTask;
}