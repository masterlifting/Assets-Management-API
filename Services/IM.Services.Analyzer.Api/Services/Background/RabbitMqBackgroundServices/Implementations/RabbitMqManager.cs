using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices.Interfaces;

using System;
using System.Text.Json;

namespace IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices.Implementations
{
    public class RabbitMqManager : IRabbitMqManager
    {
        private readonly AnalyzerContext context;
        public RabbitMqManager(AnalyzerContext context) => this.context = context;
        
        public bool CreateTickerAsync(string ticker)
        {
            //var _ticker = JsonSerializer.Deserialize<Ticker>(ticker);
            Console.WriteLine("Analyzer recived");
            return true;
        }
    }
}
