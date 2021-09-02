using IM.Services.Analyzer.Api.Services.CalculatorServices;

using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.BackgroundServices
{
    public class CalculatorBackgroundService : BackgroundService
    {
        private readonly ReportCalculator reportCalculator;
        private readonly PriceCalculator priceCalculator;

        public CalculatorBackgroundService(ReportCalculator reportCalculator, PriceCalculator priceCalculator)
        {
            this.reportCalculator = reportCalculator;
            this.priceCalculator = priceCalculator;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = new TimeSpan(0, 5, 0);

            while (true)
            {
                await priceCalculator.CalculateAsync();
                await reportCalculator.CalculateAsync();

                await Task.Delay(delay, stoppingToken);
            }
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
