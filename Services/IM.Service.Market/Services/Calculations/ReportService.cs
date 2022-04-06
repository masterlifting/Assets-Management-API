using IM.Service.Common.Net.RabbitServices;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;

using System.Runtime.Serialization;

namespace IM.Service.Market.Services.Calculations;

public class ReportService
{
    private readonly Repository<Report> repository;
    public ReportService(Repository<Report> repository) => this.repository = repository;

    public async Task SetStatusAsync(string data, byte statusId)
    {
        if (!RabbitHelper.TrySerialize(data, out Report? report))
            throw new SerializationException(nameof(Report));

        report!.StatusId = statusId;

        await repository.UpdateAsync(report, nameof(SetStatusAsync));
    }
    public async Task SetStatusRangeAsync(string data, byte statusId)
    {
        if (!RabbitHelper.TrySerialize(data, out Report[]? reports))
            throw new SerializationException(nameof(Report));

        foreach (var report in reports!)
            report.StatusId = statusId;

        await repository.UpdateAsync(reports, nameof(SetStatusRangeAsync));
    }
}