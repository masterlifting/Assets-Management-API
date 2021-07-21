namespace IM.Gateways.Web.Companies.Api.Settings.Rabbitmq
{
    public class RabbitmqSettings : BaseServiceSettings
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } =null!;

        public RabbitmqQueueSettings QueueCompaniesPrices { get; set; } = null!;
        public RabbitmqQueueSettings QueueCompaniesReports { get; set; } = null!;
        public RabbitmqQueueSettings QueueAnalyzer { get; set; } = null!;
    }
    public class RabbitmqQueueSettings
    {
        public string Name { get; set; } = null!;
    }
}
