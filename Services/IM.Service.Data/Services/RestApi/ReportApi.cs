using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Data.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Data.Services.RestApi;

public class ReportApi
{
    private readonly string rabbitConnectionString;
    public ReportApi(IOptions<ServiceSettings> options) => rabbitConnectionString = options.Value.ConnectionStrings.Mq;

    public string Load()
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Reports, QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load reports is running...";
    }
    public string Load(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Report, QueueActions.Call, companyId);
        return $"Load reports for '{companyId}' is running...";
    }
}