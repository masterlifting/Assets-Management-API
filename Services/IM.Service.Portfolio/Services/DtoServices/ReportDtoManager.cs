using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Portfolio.Models.Dto.Mq;
using IM.Service.Portfolio.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IM.Service.Portfolio.Services.DtoServices;

public class ReportDtoManager
{
    private readonly string rabbitConnectionString;
    public ReportDtoManager(IOptions<ServiceSettings> options) => rabbitConnectionString = options.Value.ConnectionStrings.Mq;

    public string Load(IFormFileCollection files, string userId)
    {
        //if (files.Count > 12)
        //    return "Max files length: 12";

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        foreach (var file in files)
        {
            var payload = new byte[file.Length];
            using var stream = file.OpenReadStream();
            var _ = stream.Read(payload, 0, (int)file.Length);

            var data = new ReportFileDto(file.FileName, file.ContentType, payload, userId);
            publisher.PublishTask(QueueNames.PortfolioData, QueueEntities.Report, QueueActions.Call, data);
        }

        return $"Load files (count: {files.Count}) is running...";
    }
}