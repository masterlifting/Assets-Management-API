using System.Collections.Generic;
using IM.Service.Portfolio.Models.Api.Mq;
using IM.Service.Portfolio.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;

using Microsoft.AspNetCore.Http;

namespace IM.Service.Portfolio.Services.Http;

public class ReportApi
{
    private readonly RabbitAction rabbitAction;
    public ReportApi(RabbitAction rabbitAction) => this.rabbitAction = rabbitAction;

    public string Load(IFormFileCollection files, string userId)
    {
        //if (files.Count > 12)
        //    return "Max files length: 12";

        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(files.Count);

        foreach (var file in files)
        {
            var payload = new byte[file.Length];
            using var stream = file.OpenReadStream();
            var _ = stream.Read(payload, 0, (int)file.Length);

            var data = new ProviderReportDto(file.FileName, file.ContentType, payload, userId);
            queueTaskParams.Add((QueueNames.Portfolio, QueueEntities.Report, QueueActions.Get, data));
        }

        rabbitAction.Publish(QueueExchanges.Function, queueTaskParams);

        return $"Load files (count: {files.Count}) is running...";
    }
}