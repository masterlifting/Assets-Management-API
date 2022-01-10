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
    private readonly AnalyzerService service;

    public CalculatorBackgroundService(ILogger<CalculatorBackgroundService> logger, AnalyzerService service)
    {
        this.logger = logger;
        this.service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken cToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(cToken))
        {
            try
            {
                await service.AnalyzeAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(ExecuteAsync), exception.Message);
            }
        }
    }
    public override Task StopAsync(CancellationToken stoppingToken)
    {
        base.StopAsync(stoppingToken);
        return Task.CompletedTask;
    }
}