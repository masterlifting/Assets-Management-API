using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.Background.RabbitMqBackgroundServices.Interfaces;

using System;
using System.Text.Json;

namespace IM.Services.Companies.Reports.Api.Services.Background.RabbitMqBackgroundServices.Implementations
{
    public class RabbitMqManager : IRabbitMqManager
    {
        private readonly ReportsContext context;
        public RabbitMqManager(ReportsContext context) => this.context = context;

        public bool CreateTickerAsync(string ticker)
        {
            //var _ticker = JsonSerializer.Deserialize<Ticker>(ticker);
            Console.WriteLine("Reports service recived");
            return true;
        }
    }
}
