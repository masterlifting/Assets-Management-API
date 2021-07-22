namespace IM.Services.Companies.Reports.Api.Services.Background.RabbitMqBackgroundServices.Interfaces
{
    public interface IRabbitMqManager
    {
        bool CreateTickerAsync(string ticker);
    }
}
