using IM.Service.Shared.Background;
using IM.Service.Market.Settings;

using Microsoft.Extensions.Options;

using static IM.Service.Shared.Helpers.LogHelper;

namespace IM.Service.Market.Services.Background.Load;

public abstract class BaseLoadBackgroundService : BackgroundService
{
    private const string methodName = "Execute";
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger logger;
    private readonly IBackgroundService task;
    private readonly string taskName;

    protected BaseLoadBackgroundService(IServiceScopeFactory scopeFactory, ILogger logger, IBackgroundService task)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.task = task;
        taskName = task.GetType().Name;
    }
    protected override async Task ExecuteAsync(CancellationToken cToken)
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(cToken))
        {
            timer.Dispose();
            timer = new(TimeSpan.FromSeconds(20));

            await using var scope = scopeFactory.CreateAsyncScope();

            var appSettings = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ServiceSettings>>();
            var taskSettings = appSettings.Value.LoadData.Tasks.FirstOrDefault(x => x.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));

            if (taskSettings is null)
            {
                logger.LogInfo(methodName, taskName, "NOT FOUND");
                continue;
            }
            if (!taskSettings.IsReady(out var info))
            {
                logger.LogTrace(methodName, taskName, info);
                continue;
            }

            try
            {
                logger.LogTrace(methodName, taskName, "START");
                await task.StartAsync(taskSettings);
                logger.LogTrace(methodName, taskName, "STOP");
            }
            catch (Exception exception)
            {
                logger.LogError(methodName, exception);
            }
            finally
            {
                timer.Dispose();
                timer = new(TimeOnly.Parse(taskSettings.WorkTime!).ToTimeSpan());
            }
        }

        timer.Dispose();
    }
    public override Task StopAsync(CancellationToken cToken)
    {
        base.StopAsync(cToken);
        return Task.CompletedTask;
    }
}