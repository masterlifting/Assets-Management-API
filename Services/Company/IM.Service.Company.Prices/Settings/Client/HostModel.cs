namespace IM.Service.Company.Prices.Settings.Client
{
    public abstract class HostModel
    {
        public string Host { get; set; } = null!;
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? ApiKey { get; set; }
    }
}