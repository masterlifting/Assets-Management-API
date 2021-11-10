using IM.Service.Common.Net.Models.Dto.Http;

namespace DataSetter.Settings.Client
{
    public class ClientSettings
    {
        public HostModel Company { get; set; } = null!;
        public HostModel CompanyData { get; set; } = null!;
        public HostModel CompanyAnalyzer { get; set; } = null!;
    }
}
