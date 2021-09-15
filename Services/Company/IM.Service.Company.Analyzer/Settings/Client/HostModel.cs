namespace IM.Service.Company.Analyzer.Settings.Client
{
    public abstract class HostModel
    {
        public string Schema { get; set; } = null!;
        public string Host { get; set; } = null!;
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Port { get; set; }
        public string Controller { get; set; } = null!;
    }
}
