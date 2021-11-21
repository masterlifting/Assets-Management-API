using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Company.Analyzer.Services.CalculatorServices;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Analyzer.Services.BackgroundServices
{
    public class CalculatorBackgroundService : BackgroundService
    {
        private readonly ILogger<CalculatorBackgroundService> logger;
        private readonly IServiceProvider services;
        private bool start = true;
        public CalculatorBackgroundService(ILogger<CalculatorBackgroundService> logger, IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = new TimeSpan(0, 1, 0);

            while (start)
            {
                start = false;
                await Task.Delay(delay, stoppingToken);

                var scope = services.CreateScope();
                var priceCalculator = scope.ServiceProvider.GetRequiredService<PriceCalculator>();
                var reportCalculator = scope.ServiceProvider.GetRequiredService<ReportCalculator>();
                var ratingCalculator = scope.ServiceProvider.GetRequiredService<RatingCalculator>();

                try
                {
                    if (await priceCalculator.CalculateAsync())
                        if (await reportCalculator.CalculateAsync())
                            await ratingCalculator.CalculateAsync();
                        else
                            await ratingCalculator.CalculateAsync();
                    else if (await reportCalculator.CalculateAsync())
                        await ratingCalculator.CalculateAsync();
                }
                catch (Exception exception)
                {
                    logger.LogError(LogEvents.Processing, "Analyzer error: {exception}", exception.InnerException?.Message ?? exception.Message);
                }

                scope.Dispose();
                start = true;
            }
            // ReSharper disable once FunctionNeverReturns
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
