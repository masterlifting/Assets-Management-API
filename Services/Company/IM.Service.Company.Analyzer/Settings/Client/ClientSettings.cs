using CommonServices.Models.Http;

namespace IM.Service.Company.Analyzer.Settings.Client
{
    public class ClientSettings
    {
        public HostModel CompanyPrices { get; set; } = null!;
        public HostModel CompanyReports { get; set; } = null!;
        public HostModel GatewayCompanies { get; set; } = null!;
    }
}
