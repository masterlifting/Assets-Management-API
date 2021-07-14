namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ServiceSettings
    {
        public PricesClientSettings PricesSettings { get; set; } = null!;
        public ReportsClientSettings ReportsSettings { get; set; } = null!;
        public AnalyzerClientSettings AnalyzerSettings { get; set; } = null!;
    }
}
