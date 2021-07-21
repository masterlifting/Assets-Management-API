namespace IM.Services.Analyzer.Api.Settings.Rabbitmq
{
    public class RabbitmqSettings
    {
        public string Host { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public RabbitmqQueueSettings QueueAnalyzer { get; set; } = null!;
    }
    public class RabbitmqQueueSettings
    {
        public string Name { get; set; } = null!;
    }
}
