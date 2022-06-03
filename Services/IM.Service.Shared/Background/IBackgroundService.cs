using System.Threading.Tasks;

namespace IM.Service.Shared.Background;

public interface IBackgroundService
{
    Task StartAsync<T>(T settings) where T : BackgroundTaskSettings;
}