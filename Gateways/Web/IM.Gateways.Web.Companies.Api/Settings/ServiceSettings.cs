namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ServiceSettings
    {
        public ClientSettings PricesSettings { get; set; } = null!;
        public ClientSettings ReportsSettings { get; set; } = null!;
        public ClientSettings AnalyzerSettings { get; set; } = null!;
    }
}
