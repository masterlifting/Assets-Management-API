using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.Background.RabbitMqBackgroundServices.Interfaces;

using System;
using System.Text.Json;

namespace IM.Services.Companies.Prices.Api.Services.Background.RabbitMqBackgroundServices.Implementations
{
    public class RabbitMqManager : IRabbitMqManager
    {
        private readonly PricesContext context;
        public RabbitMqManager(PricesContext context) => this.context = context;
        
        public bool CreateTickerAsync(string ticker)
        {
            //var _ticker = JsonSerializer.Deserialize<Ticker>(ticker);
            Console.WriteLine("Prices service recived");
            return true;
        }
    }
}
