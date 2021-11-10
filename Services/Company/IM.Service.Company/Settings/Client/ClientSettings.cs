using IM.Service.Common.Net.Models.Dto.Http;

namespace IM.Service.Company.Settings.Client
{
    public class ClientSettings
    {
        public HostModel CompanyData { get; set; } = null!;
        public HostModel CompanyAnalyzer { get; set; } = null!;
    }
}
