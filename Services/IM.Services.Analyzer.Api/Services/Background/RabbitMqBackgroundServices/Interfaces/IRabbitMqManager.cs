namespace IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices.Interfaces
{
    public interface IRabbitMqManager
    {
        bool CreateTickerAsync(string ticker);
    }
}
