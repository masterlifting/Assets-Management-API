namespace IM.Service.Company.Reports.Settings.Client
{
    public abstract class InvestingModel : HostModel
    {
        public string Path { get; set; } = null!;
        public string Financial { get; set; } = null!;
        public string Balance { get; set; } = null!;
        public string Dividends { get; set; } = null!;
    }
}
