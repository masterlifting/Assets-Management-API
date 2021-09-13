using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;
using IM.Services.Company.Analyzer.Services.CalculatorServices;

namespace IM.Services.Company.Analyzer.Services.BackgroundServices
{
    public class CalculatorBackgroundService : BackgroundService
    {
        private readonly IServiceProvider services;
        public CalculatorBackgroundService(IServiceProvider services) => this.services = services;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = new TimeSpan(0, 0, 10);

            while (true)
            {
                await Task.Delay(delay, stoppingToken);

                var scope = services.CreateScope();
                var reportCalculator = scope.ServiceProvider.GetRequiredService<ReportCalculator>();
                var priceCalculator = scope.ServiceProvider.GetRequiredService<PriceCalculator>();
                var ratingCalculator = scope.ServiceProvider.GetRequiredService<RatingCalculator>();

                try
                {
                    var priceResult = await priceCalculator.CalculateAsync();
                    var reportResult = await reportCalculator.CalculateAsync();
                    
                    if (priceResult || reportResult)
                        await ratingCalculator.CalculateAsync();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Analyzer exception: {ex.InnerException?.Message ?? ex.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                scope.Dispose();
            }
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
