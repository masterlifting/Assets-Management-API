namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ClientSettings
    {
        public string Sсhema { get => "http"; }
        public string Host { get; set; } = null!;
        public int Port { get; set; }
    }
}
