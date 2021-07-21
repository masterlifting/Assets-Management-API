using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background.RabbitMqBackgroundServices.Interfaces
{
    public interface IRabbitMqManager
    {
        bool CreateTickerAsync(string ticker);
    }
}
