namespace CommonServices.Models.Http
{
    public class HostModel
    {
        public string Schema { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string Controller { get; set; } = null!;
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? ApiKey { get; set; }
    }
}
