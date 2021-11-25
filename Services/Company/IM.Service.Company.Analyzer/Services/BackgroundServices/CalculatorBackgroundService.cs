using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Company.Analyzer.Services.CalculatorServices;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Analyzer.Services.BackgroundServices;

public class CalculatorBackgroundService : BackgroundService
{
    private readonly ILogger<CalculatorBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private bool start = true;
    public CalculatorBackgroundService(ILogger<CalculatorBackgroundService> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = new TimeSpan(0, 1, 0);

        while (start)
        {
            start = false;
            await Task.Delay(delay, stoppingToken);

            var serviceProvider = scopeFactory.CreateScope().ServiceProvider;
            var priceCalculator = serviceProvider.GetRequiredService<PriceCalculator>();
            var reportCalculator = serviceProvider.GetRequiredService<ReportCalculator>();
            var ratingCalculator = serviceProvider.GetRequiredService<RatingCalculator>();

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